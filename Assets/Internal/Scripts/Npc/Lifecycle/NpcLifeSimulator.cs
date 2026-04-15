using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Events;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Behavior;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Npc.Names;
using Internal.Scripts.Npc.Routing;
using Internal.Scripts.Npc.Save;
using Internal.Scripts.Npc.Trading;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Save;
using Internal.Scripts.UI.Localization;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Npc.Lifecycle
{
    public sealed class NpcLifeSimulator : IInitializable, IDisposable
    {
        private readonly NpcSimulationSettings _settings;
        private readonly NpcFactory _factory;
        private readonly IRoadNodeLookup _nodeLookup;
        private readonly IRoadNetwork _roadNetwork;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly NpcSupplyPlanner _supplyPlanner;
        private readonly NpcRouteDecisionService _routeDecisionService;
        private readonly NameDatabase _nameDatabase;
        private readonly SaveRepository _saveRepository;
        private readonly DayTracker _dayTracker;
        private readonly NpcSimulation _simulation;
        private readonly NpcCityVisitProcessor _visitProcessor;
        private readonly NpcDayProcessor _dayProcessor;
        private readonly NpcSpawnRoller _spawnRoller;
        private readonly NpcGuildTradeService _guildTradeService;

        private const int ActivityLogCapacity = 100;

        private readonly List<NpcCaravanAgent> _agents = new();
        private readonly List<string> _activityLog = new();
        private List<string> _nodeIds;
        private List<string> _cityNodeIds;

        private readonly Dictionary<NpcArchetype, int> _archetypeCounts = new();
        private readonly List<NpcCaravanAgent> _daySnapshot = new();
        private readonly List<NpcEconomyState> _dayEconomies = new();
        private readonly List<string> _dayDestinations = new();
        private int[] _daySupplies = new int[16];
        private readonly NpcDayContext _dayContext = new();
        private Func<int, float> _estimateDaysToCityDelegate;
        private Func<double> _nextRandomDelegate;

        public IReadOnlyList<NpcCaravanAgent> Agents => _agents;
        public IReadOnlyList<string> ActivityLog => _activityLog;

        public event Action OnTradeCompleted;

        public NpcLifeSimulator(
            NpcSimulationSettings settings,
            NpcFactory factory,
            IRoadNodeLookup nodeLookup,
            IRoadNetwork roadNetwork,
            ICityNodeResolver cityNodeResolver,
            NpcSupplyPlanner supplyPlanner,
            NpcRouteDecisionService routeDecisionService,
            NameDatabase nameDatabase,
            SaveRepository saveRepository,
            DayTracker dayTracker,
            NpcSimulation simulation,
            NpcCityVisitProcessor visitProcessor,
            NpcDayProcessor dayProcessor,
            NpcGuildTradeService guildTradeService)
        {
            _settings = settings;
            _factory = factory;
            _nodeLookup = nodeLookup;
            _roadNetwork = roadNetwork;
            _cityNodeResolver = cityNodeResolver;
            _supplyPlanner = supplyPlanner;
            _routeDecisionService = routeDecisionService;
            _nameDatabase = nameDatabase;
            _saveRepository = saveRepository;
            _dayTracker = dayTracker;
            _simulation = simulation;
            _visitProcessor = visitProcessor;
            _dayProcessor = dayProcessor;
            _spawnRoller = new NpcSpawnRoller(settings);
            _guildTradeService = guildTradeService;
        }

        public void Initialize()
        {
            _guildTradeService?.SetActivityLog(AppendActivityLog);

            _nodeIds = new List<string>();
            foreach (string nodeId in _nodeLookup.Nodes.Keys)
            {
                if (_roadNetwork.GetOutgoingSegments(nodeId).Count > 0)
                    _nodeIds.Add(nodeId);
            }
            _cityNodeIds = new List<string>();
            foreach (string nodeId in _nodeIds)
            {
                if (_cityNodeResolver.TryGetCityByNodeId(nodeId, out _))
                    _cityNodeIds.Add(nodeId);
            }

            if (_cityNodeIds.Count < 2)
            {
                Debug.LogWarning("[NpcLifeSimulator] Not enough city nodes to simulate NPCs.");
                return;
            }

            _estimateDaysToCityDelegate = i => EstimateDaysToNearestCity(_daySnapshot[i]);
            _nextRandomDelegate = () => UnityEngine.Random.value;

            NpcSaveData savedNpcs = _saveRepository.Data.Npcs;
            if (savedNpcs != null && savedNpcs.Agents.Count > 0)
            {
                RestoreAgents(savedNpcs);
            }
            else
            {
                int count = Mathf.Max(1, _settings.AgentCount);
                for (int i = 0; i < count; i++)
                    TrySpawnAgent();
            }

            _dayTracker.OnDayChanged += HandleDayChanged;
        }

        public void Dispose()
        {
            _dayTracker.OnDayChanged -= HandleDayChanged;

            foreach (NpcCaravanAgent agent in _agents)
            {
                if (agent != null)
                    agent.OnArrived -= HandleArrival;
            }
            _agents.Clear();
        }

        public NpcSaveData BuildSaveData()
        {
            NpcSaveData data = new NpcSaveData();
            foreach (NpcCaravanAgent agent in _agents)
            {
                if (agent == null)
                    continue;

                data.Agents.Add(new NpcAgentSaveState
                {
                    Name = agent.EconomyState.Name,
                    NameId = agent.EconomyState.NameId,
                    Money = agent.EconomyState.Money,
                    Inventory = agent.EconomyState.Inventory,
                    CapacityKg = agent.EconomyState.CapacityKg,
                    CurrentNodeId = agent.CurrentNodeId,
                    DestinationNodeId = agent.DestinationNodeId,
                    SpeedMetersPerDay = agent.SpeedMetersPerDay,
                    ColorIndex = agent.ColorIndex,
                    PrefabIndex = agent.PrefabIndex,
                    Archetype = agent.EconomyState.Archetype,
                    Experience = agent.EconomyState.Experience,
                    Debt = agent.EconomyState.Debt,
                    InDebt = agent.EconomyState.InDebt,
                    Purchases = agent.EconomyState.Purchases,
                    Knowledge = agent.EconomyState.Knowledge,
                    LastForageDay = agent.EconomyState.LastForageDay,
                    ActiveContract = agent.EconomyState.ActiveContract
                });
            }
            return data;
        }

        private void RestoreAgents(NpcSaveData savedNpcs)
        {
            foreach (NpcAgentSaveState state in savedNpcs.Agents)
            {
                if (string.IsNullOrWhiteSpace(state.CurrentNodeId))
                    continue;

                if (!_nodeLookup.TryGetTransform(state.CurrentNodeId, out _))
                {
                    Debug.LogWarning($"[NpcLifeSimulator] Saved node '{state.CurrentNodeId}' not found. Skipping '{state.Name}'.");
                    continue;
                }

                RoadAgentConfig config = new RoadAgentConfig
                {
                    SpeedMetersPerDay = state.SpeedMetersPerDay,
                    Lane = RoadLane.Right,
                    LateralOffsetMeters = 0f
                };

                NpcEconomyState economy = new NpcEconomyState(
                    state.Name, state.Money, state.CapacityKg);
                economy.NameId = state.NameId;
                economy.Inventory = state.Inventory ?? new InventoryState
                    { Items = new List<ItemStackState>() };
                economy.Archetype = state.Archetype;
                economy.Experience = state.Experience;
                economy.Debt = state.Debt;
                economy.InDebt = state.InDebt;
                economy.Purchases = state.Purchases ?? new List<PurchaseRecord>();
                economy.Knowledge = state.Knowledge ?? new NpcKnowledgeState();
                economy.LastForageDay = state.LastForageDay;
                economy.ActiveContract = state.ActiveContract;

                NpcCaravanAgent agent = _factory.CreateCaravan(
                    config, state.CurrentNodeId, economy, state.NameId,
                    state.PrefabIndex, state.ColorIndex);
                if (agent == null)
                    continue;

                agent.OnArrived += HandleArrival;
                _agents.Add(agent);

                if (!string.IsNullOrWhiteSpace(state.DestinationNodeId))
                {
                    agent.SetDestination(state.DestinationNodeId);
                }
                else if (TryChoosePlannedTarget(
                    state.CurrentNodeId,
                    economy,
                    state.SpeedMetersPerDay,
                    () => UnityEngine.Random.value,
                    out string target))
                {
                    agent.SetDestination(target);
                }
            }

            int deficit = Mathf.Max(0, _settings.AgentCount - _agents.Count);
            for (int i = 0; i < deficit; i++)
                TrySpawnAgent();

            Debug.Log($"[NpcLifeSimulator] Restored {_agents.Count} agents from save.");
        }

        private void TrySpawnAgent()
        {
            if (!TryChooseSpawnStartNode(out string start))
            {
                Debug.LogWarning("[NpcLifeSimulator] Failed to spawn agent.");
                return;
            }

            RoadAgentConfig config = BuildRandomConfig();

            NameEntry nameEntry = _nameDatabase != null ? _nameDatabase.GetRandom() : null;
            string agentName = nameEntry?.Name != null
                ? LocalizationService.ResolveString(nameEntry.Name, nameEntry.Id, "NpcLife.Name")
                : $"Trader_{_agents.Count}";

            NpcArchetype archetype = RollArchetype();
            NpcExperienceLevel experience = _spawnRoller.RollExperience(
                (min, max) => UnityEngine.Random.Range(min, max));

            int money;
            float capacity;

            if (_settings.Archetypes != null && _settings.Archetypes.Count > 0)
            {
                NpcArchetypeDefinition archDef = _spawnRoller.FindArchetypeDef(archetype);
                money = UnityEngine.Random.Range(archDef.MoneyRange.x, archDef.MoneyRange.y + 1);
                capacity = UnityEngine.Random.Range(archDef.CapacityRange.x, archDef.CapacityRange.y);
            }
            else
            {
                money = UnityEngine.Random.Range(_settings.MoneyRange.x, _settings.MoneyRange.y + 1);
                capacity = UnityEngine.Random.Range(_settings.CapacityRange.x, _settings.CapacityRange.y);
            }

            NpcEconomyState economy = new NpcEconomyState(agentName, money, capacity);
            economy.NameId = nameEntry?.Id;
            economy.Archetype = archetype;
            economy.Experience = experience;

            if (!TryChoosePlannedTarget(
                start,
                economy,
                config.SpeedMetersPerDay,
                () => UnityEngine.Random.value,
                out string target))
            {
                Debug.LogWarning("[NpcLifeSimulator] Failed to choose initial planned target.");
                return;
            }

            int startSupplies = EstimateSuppliesForTrip(start, target, config.SpeedMetersPerDay);
            InventoryStateMutator.AddItems(
                economy.Inventory, SuppliesItemId.Value, startSupplies);

            NpcCaravanAgent agent = _factory.CreateCaravan(config, start, economy, nameEntry?.Id);
            if (agent == null)
            {
                Debug.LogWarning("[NpcLifeSimulator] Failed to spawn agent.");
                return;
            }

            agent.OnArrived += HandleArrival;
            _agents.Add(agent);
            agent.SetDestination(target);

            AppendActivityLog($"Day {_dayTracker.CurrentDay}: Spawned '{agentName}' [{archetype}] from {start} to {target}");
        }

        private void HandleArrival(NpcCaravanAgent agent)
        {
            if (agent == null || _cityNodeIds.Count < 2)
                return;

            if (!_cityNodeResolver.TryGetCityByNodeId(agent.CurrentNodeId, out CityData city))
            {
                if (TryChooseTarget(agent.CurrentNodeId, out string target))
                    agent.SetDestination(target);
                return;
            }

            var context = new NpcCityVisitContext
            {
                Economy = agent.EconomyState,
                City = city,
                CurrentNodeId = agent.CurrentNodeId,
                SpeedMetersPerDay = agent.SpeedMetersPerDay,
                CurrentDay = _dayTracker.CurrentDay,
                NextRandom = () => UnityEngine.Random.value,
                RouteEnvironment = new RuntimeRouteDecisionEnvironment(this)
            };

            _visitProcessor.Process(context);

            if (!string.IsNullOrEmpty(context.NextTargetNodeId))
            {
                agent.SetDestination(context.NextTargetNodeId);

                AppendActivityLog($"Day {_dayTracker.CurrentDay}: {agent.EconomyState.Name} traded at {city.Id}, money={agent.EconomyState.Money}");

                if (context.BuyStats.BoughtUnits > 0 || context.SellStats.SoldUnits > 0)
                    OnTradeCompleted?.Invoke();
            }
            else
            {
                if (context.SellStats.SoldUnits > 0)
                    OnTradeCompleted?.Invoke();
            }
        }

        private void HandleDayChanged(int day)
        {
            _daySnapshot.Clear();
            _daySnapshot.AddRange(_agents);

            _dayEconomies.Clear();
            _dayDestinations.Clear();
            for (int i = 0; i < _daySnapshot.Count; i++)
            {
                var a = _daySnapshot[i];
                _dayEconomies.Add(a?.EconomyState);
                _dayDestinations.Add(a?.DestinationNodeId ?? string.Empty);
            }

            if (_daySupplies.Length < _daySnapshot.Count)
                Array.Resize(ref _daySupplies, _daySnapshot.Count);

            _dayContext.Economies = _dayEconomies;
            _dayContext.DestinationNodeIds = _dayDestinations;
            _dayContext.CurrentDay = day;
            _dayContext.SuppliesSnapshot = _daySupplies;
            _dayContext.EstimateDaysToCity = _estimateDaysToCityDelegate;
            _dayContext.NextRandom = _nextRandomDelegate;
            _dayContext.DeadIndices.Clear();
            _dayContext.ForagedIndices.Clear();

            for (int i = 0; i < _daySnapshot.Count; i++)
                _daySupplies[i] = InventoryStateMutator.GetItemCount(
                    _dayEconomies[i]?.Inventory, SuppliesItemId.Value);

            _dayProcessor.ProcessDay(_dayContext);

            for (int i = 0; i < _daySnapshot.Count; i++)
            {
                NpcCaravanAgent agent = _daySnapshot[i];
                if (agent?.RoadAgent == null || string.IsNullOrEmpty(agent.DestinationNodeId))
                    continue;
                if (!_dayContext.ForagedIndices.Contains(i))
                    agent.RoadAgent.AdvanceByDays(1f);
            }

            foreach (int deadIdx in _dayContext.DeadIndices)
            {
                NpcCaravanAgent agent = _daySnapshot[deadIdx];
                AppendActivityLog($"Day {_dayTracker.CurrentDay}: '{agent.EconomyState.Name}' died (no supplies)");
                KillAgent(agent);
                TrySpawnAgent();
            }
        }

        public void DebugKillAgent(NpcCaravanAgent agent) => KillAgent(agent);
        public void DebugSpawnAgent() => TrySpawnAgent();

        private NpcArchetype RollArchetype()
        {
            _archetypeCounts.Clear();
            for (int i = 0; i < _agents.Count; i++)
            {
                var a = _agents[i];
                if (a == null) continue;
                var arch = a.EconomyState.Archetype;
                _archetypeCounts.TryGetValue(arch, out int c);
                _archetypeCounts[arch] = c + 1;
            }
            return _spawnRoller.ChooseArchetypeToSpawn(
                arch => _archetypeCounts.TryGetValue(arch, out int c) ? c : 0);
        }

        private void AppendActivityLog(string line)
        {
            _activityLog.Add(line);
            if (_activityLog.Count > ActivityLogCapacity)
                _activityLog.RemoveAt(0);
        }

        private void KillAgent(NpcCaravanAgent agent)
        {
            agent.OnArrived -= HandleArrival;
            _simulation.Unregister(agent.RoadAgent);
            agent.DestroyView();
            agent.Dispose();
            _agents.Remove(agent);
        }

        private bool TryChooseSpawnStartNode(out string start)
        {
            start = null;
            if (_cityNodeIds == null || _cityNodeIds.Count < 2)
                return false;

            start = _cityNodeIds[UnityEngine.Random.Range(0, _cityNodeIds.Count)];
            return !string.IsNullOrEmpty(start);
        }

        private bool TryChoosePlannedTarget(
            string currentNodeId,
            NpcEconomyState economyState,
            float speedMetersPerDay,
            Func<float> nextRandomValue,
            out string target)
        {
            target = null;
            if (string.IsNullOrEmpty(currentNodeId) ||
                !_cityNodeResolver.TryGetCityByNodeId(currentNodeId, out CityData city))
            {
                return TryChooseTarget(currentNodeId, out target);
            }

            NpcRouteDecisionResult routeDecision = _routeDecisionService.ChooseNextTarget(
                new NpcRouteDecisionContext(
                    economyState,
                    currentNodeId,
                    city.Id,
                    speedMetersPerDay,
                    _dayTracker.CurrentDay),
                new RuntimeRouteDecisionEnvironment(this),
                nextRandomValue);

            if (!routeDecision.HasTarget)
                return TryChooseTarget(currentNodeId, out target);

            target = routeDecision.TargetNodeId;
            return true;
        }

        private bool TryChooseTarget(string currentNodeId, out string target)
        {
            target = null;
            if (_cityNodeIds == null || _cityNodeIds.Count < 2)
                return false;

            for (int attempt = 0; attempt < 5; attempt++)
            {
                string candidate = _cityNodeIds[UnityEngine.Random.Range(0, _cityNodeIds.Count)];
                if (candidate != currentNodeId)
                {
                    target = candidate;
                    return true;
                }
            }
            return false;
        }

        private int EstimateSuppliesForTrip(string fromNodeId, string toNodeId, float speed)
        {
            if (!_nodeLookup.TryGetTransform(fromNodeId, out Transform from) ||
                !_nodeLookup.TryGetTransform(toNodeId, out Transform to) ||
                speed <= 0f)
                return _settings.StartingSupplies;

            float dist = Vector3.Distance(from.position, to.position) * _settings.RoadWindingFactor;
            int estimatedDays = Mathf.CeilToInt(dist / speed);
            return (estimatedDays + _settings.ExtraSuppliesDays) * _settings.SuppliesPerDay;
        }

        private float EstimateDaysToNearestCity(NpcCaravanAgent agent)
        {
            if (!_nodeLookup.TryGetTransform(agent.CurrentNodeId, out Transform from))
                return 1f;

            Vector3 pos = from.position;
            float minDistSqr = float.MaxValue;

            foreach (string cityNodeId in _cityNodeIds)
            {
                if (!_nodeLookup.TryGetTransform(cityNodeId, out Transform t))
                    continue;
                float distSqr = (t.position - pos).sqrMagnitude;
                if (distSqr < minDistSqr)
                    minDistSqr = distSqr;
            }

            if (minDistSqr >= float.MaxValue)
                return 1f;

            float euclidean = Mathf.Sqrt(minDistSqr);
            float roadDist = euclidean * _settings.RoadWindingFactor;
            return Mathf.Max(1f, roadDist / agent.SpeedMetersPerDay);
        }

        private RoadAgentConfig BuildRandomConfig()
        {
            float min = Mathf.Min(_settings.SpeedRangeMetersPerDay.x, _settings.SpeedRangeMetersPerDay.y);
            float max = Mathf.Max(_settings.SpeedRangeMetersPerDay.x, _settings.SpeedRangeMetersPerDay.y);
            float speed = Mathf.Lerp(min, max, UnityEngine.Random.value);

            return new RoadAgentConfig
            {
                SpeedMetersPerDay = speed,
                Lane = RoadLane.Right,
                LateralOffsetMeters = 0f
            };
        }

        private sealed class RuntimeRouteDecisionEnvironment : INpcRouteDecisionEnvironment
        {
            private readonly NpcLifeSimulator _owner;

            public RuntimeRouteDecisionEnvironment(NpcLifeSimulator owner)
            {
                _owner = owner;
            }

            public IReadOnlyList<string> CityNodeIds => _owner._cityNodeIds;

            public bool TryGetCityByNodeId(string nodeId, out CityData city)
            {
                return _owner._cityNodeResolver.TryGetCityByNodeId(nodeId, out city);
            }

            public float EstimateTravelDays(string fromNodeId, string toNodeId, float speedMetersPerDay)
            {
                int suppliesNeeded = _owner._supplyPlanner.EstimateSuppliesNeeded(
                    fromNodeId, toNodeId, speedMetersPerDay);
                if (suppliesNeeded < 0 || speedMetersPerDay <= 0f)
                {
                    if (!_owner._nodeLookup.TryGetTransform(fromNodeId, out Transform from) ||
                        !_owner._nodeLookup.TryGetTransform(toNodeId, out Transform to))
                    {
                        return 1f;
                    }

                    float dist = Vector3.Distance(from.position, to.position) * _owner._settings.RoadWindingFactor;
                    return Mathf.Max(1f, dist / speedMetersPerDay);
                }

                return Mathf.Max(1f, (float)suppliesNeeded / _owner._settings.SuppliesPerDay);
            }

            public string FindNearestCityNode(string currentNodeId)
            {
                List<string> candidates = new List<string>(_owner._cityNodeIds);
                candidates.Remove(currentNodeId);
                return _owner._nodeLookup.FindNearestAmong(currentNodeId, candidates);
            }
        }
    }
}

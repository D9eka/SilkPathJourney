using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Events;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Utils;
using Internal.Scripts.Npc.Names;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.Npc.Save;
using Internal.Scripts.Npc.Trading;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Save;
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
        private readonly NpcTrader _trader;
        private readonly NpcSupplyPlanner _supplyPlanner;
        private readonly NameDatabase _nameDatabase;
        private readonly SaveRepository _saveRepository;
        private readonly DayTracker _dayTracker;
        private readonly NpcSimulation _simulation;

        private readonly List<NpcCaravanAgent> _agents = new();
        private List<string> _nodeIds;
        private List<string> _cityNodeIds;

        public IReadOnlyList<NpcCaravanAgent> Agents => _agents;

        public event Action OnTradeCompleted;

        public NpcLifeSimulator(
            NpcSimulationSettings settings,
            NpcFactory factory,
            IRoadNodeLookup nodeLookup,
            IRoadNetwork roadNetwork,
            ICityNodeResolver cityNodeResolver,
            NpcTrader trader,
            NpcSupplyPlanner supplyPlanner,
            NameDatabase nameDatabase,
            SaveRepository saveRepository,
            DayTracker dayTracker,
            NpcSimulation simulation)
        {
            _settings = settings;
            _factory = factory;
            _nodeLookup = nodeLookup;
            _roadNetwork = roadNetwork;
            _cityNodeResolver = cityNodeResolver;
            _trader = trader;
            _supplyPlanner = supplyPlanner;
            _nameDatabase = nameDatabase;
            _saveRepository = saveRepository;
            _dayTracker = dayTracker;
            _simulation = simulation;
        }

        public void Initialize()
        {
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
                    PrefabIndex = agent.PrefabIndex
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

                NpcCaravanAgent agent = _factory.CreateCaravan(
                    config, state.CurrentNodeId, economy, state.NameId,
                    state.PrefabIndex, state.ColorIndex);
                if (agent == null)
                    continue;

                agent.OnArrived += HandleArrival;
                _agents.Add(agent);

                if (!string.IsNullOrWhiteSpace(state.DestinationNodeId))
                    agent.SetDestination(state.DestinationNodeId);
                else if (TryChooseTarget(state.CurrentNodeId, out string target))
                    agent.SetDestination(target);
            }

            int deficit = Mathf.Max(0, _settings.AgentCount - _agents.Count);
            for (int i = 0; i < deficit; i++)
                TrySpawnAgent();

            Debug.Log($"[NpcLifeSimulator] Restored {_agents.Count} agents from save.");
        }

        private void TrySpawnAgent()
        {
            if (!TryChooseTwoNodes(out string start, out string target))
            {
                Debug.LogWarning("[NpcLifeSimulator] Failed to spawn agent.");
                return;
            }

            RoadAgentConfig config = BuildRandomConfig();

            NameEntry nameEntry = _nameDatabase != null ? _nameDatabase.GetRandom() : null;
            string agentName = nameEntry?.Name != null
                ? LocalizationService.ResolveString(nameEntry.Name, nameEntry.Id, "NpcLife.Name")
                : $"Trader_{_agents.Count}";

            int money = UnityEngine.Random.Range(_settings.MoneyRange.x, _settings.MoneyRange.y + 1);
            float capacity = UnityEngine.Random.Range(_settings.CapacityRange.x, _settings.CapacityRange.y);

            NpcEconomyState economy = new NpcEconomyState(agentName, money, capacity);
            economy.NameId = nameEntry?.Id;
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

            Debug.Log($"[NpcLifeSimulator] Spawned '{agentName}' money={money} cap={capacity:F0} from {start} to {target}");
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

            string nextTarget = ChooseAffordableTarget(agent, city.Id);

            _trader.ExecuteTrade(agent.EconomyState, city.Id,
                agent.CurrentNodeId, nextTarget, agent.SpeedMetersPerDay);

            agent.SetDestination(nextTarget);

            Debug.Log($"[NpcLifeSimulator] '{agent.EconomyState.Name}' traded at {city.Id}. " +
                      $"Money={agent.EconomyState.Money}, heading to {nextTarget}");

            OnTradeCompleted?.Invoke();
        }

        private string ChooseAffordableTarget(NpcCaravanAgent agent, string cityId)
        {
            List<string> candidates = new List<string>(_cityNodeIds);
            candidates.Remove(agent.CurrentNodeId);
            candidates.Shuffle();

            foreach (string nodeId in candidates)
            {
                if (_supplyPlanner.CanAffordTrip(agent.EconomyState, cityId,
                        agent.CurrentNodeId, nodeId, agent.SpeedMetersPerDay))
                    return nodeId;
            }

            return _nodeLookup.FindNearestAmong(agent.CurrentNodeId, candidates);
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

        private void HandleDayChanged(int day)
        {
            List<NpcCaravanAgent> dead = null;

            foreach (NpcCaravanAgent agent in _agents)
            {
                if (agent == null || string.IsNullOrEmpty(agent.DestinationNodeId))
                    continue;

                int suppliesCount = InventoryStateMutator.GetItemCount(
                    agent.EconomyState.Inventory, SuppliesItemId.Value);
                if (suppliesCount > 0)
                {
                    int toConsume = Mathf.Min(_settings.SuppliesPerDay, suppliesCount);
                    InventoryStateMutator.RemoveItems(
                        agent.EconomyState.Inventory, SuppliesItemId.Value, toConsume);
                }
                else
                {
                    float daysToCity = EstimateDaysToNearestCity(agent);
                    float dailyDeath = daysToCity > 0f
                        ? 1f - Mathf.Pow(_settings.StarvationSurvivalChance, 1f / daysToCity)
                        : 1f;

                    if (UnityEngine.Random.value < dailyDeath)
                    {
                        dead ??= new List<NpcCaravanAgent>();
                        dead.Add(agent);
                    }
                }
            }

            if (dead != null)
            {
                foreach (NpcCaravanAgent agent in dead)
                {
                    Debug.Log($"[NpcLifeSimulator] '{agent.EconomyState.Name}' died (no supplies).");
                    KillAgent(agent);
                    TrySpawnAgent();
                }
            }
        }

        private void KillAgent(NpcCaravanAgent agent)
        {
            agent.OnArrived -= HandleArrival;
            _simulation.Unregister(agent.RoadAgent);
            agent.DestroyView();
            agent.Dispose();
            _agents.Remove(agent);
        }

        private bool TryChooseTwoNodes(out string start, out string target)
        {
            start = null;
            target = null;

            if (_cityNodeIds == null || _cityNodeIds.Count < 2)
                return false;

            int a = UnityEngine.Random.Range(0, _cityNodeIds.Count);
            int b;
            do
            {
                b = UnityEngine.Random.Range(0, _cityNodeIds.Count);
            } while (b == a && _cityNodeIds.Count > 1);

            start = _cityNodeIds[a];
            target = _cityNodeIds[b];
            return start != target;
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
    }
}

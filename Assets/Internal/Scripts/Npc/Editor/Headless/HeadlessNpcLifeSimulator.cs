using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Npc.Behavior;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Npc.Routing;
using Internal.Scripts.Npc.Trading;
using Internal.Scripts.Road.Graph;
using UnityEngine;

namespace Internal.Scripts.Npc.Editor.Headless
{
    public sealed class HeadlessNpcLifeSimulator
    {
        private readonly NpcSimulationSettings _settings;
        private readonly RoadGraphSnapshot _snapshot;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly NpcRouteDecisionService _routeDecisionService;
        private readonly NpcCityVisitProcessor _visitProcessor;
        private readonly NpcDayProcessor _dayProcessor;
        private readonly System.Random _rng;

        private const int TradeLogCapacity = 100;

        private readonly List<HeadlessCaravanAgent> _agents = new();
        private readonly List<string> _tradeLog = new();
        private List<string> _cityNodeIds;
        private int _currentDay;
        private HeadlessNpcStatistics _statistics;
        private HeadlessNpcSpawner _spawner;
        private HeadlessRouteDecisionEnvironment _routeEnv;

        public IReadOnlyList<HeadlessCaravanAgent> Agents => _agents;
        public IReadOnlyList<string> TradeLog => _tradeLog;
        public HeadlessNpcStatistics Statistics => _statistics;

        public event Action OnTradeCompleted;

        public int TotalArrivals => _statistics.TotalArrivals;
        public int TotalTrades => _statistics.TotalTrades;
        public int TotalSuppliesBoughtUnits => _statistics.TotalSuppliesBoughtUnits;
        public int TotalGoodsBoughtUnits => _statistics.TotalGoodsBoughtUnits;
        public int TotalGoodsSoldUnits => _statistics.TotalGoodsSoldUnits;
        public int TotalMoneySpentOnSupplies => _statistics.TotalMoneySpentOnSupplies;
        public int TotalMoneySpentOnGoods => _statistics.TotalMoneySpentOnGoods;
        public int TotalMoneyEarnedFromGoods => _statistics.TotalMoneyEarnedFromGoods;
        public int TotalContractsCompleted => _statistics.TotalContractsCompleted;
        public int TotalContractRewards => _statistics.TotalContractRewards;

        public HeadlessNpcLifeSimulator(
            NpcSimulationSettings settings,
            RoadGraphSnapshot snapshot,
            ICityNodeResolver cityNodeResolver,
            NpcRouteDecisionService routeDecisionService,
            NpcCityVisitProcessor visitProcessor,
            NpcDayProcessor dayProcessor,
            int seed)
        {
            _settings = settings;
            _snapshot = snapshot;
            _cityNodeResolver = cityNodeResolver;
            _routeDecisionService = routeDecisionService;
            _visitProcessor = visitProcessor;
            _dayProcessor = dayProcessor;
            _rng = new System.Random(seed);
        }

        public void Initialize(IRoadNetwork roadNetwork)
        {
            _statistics = new HeadlessNpcStatistics();

            var nodeIds = new List<string>();
            foreach (string nodeId in roadNetwork.Nodes)
            {
                if (roadNetwork.GetOutgoingSegments(nodeId).Count > 0)
                    nodeIds.Add(nodeId);
            }

            _cityNodeIds = new List<string>();
            foreach (string nodeId in nodeIds)
            {
                if (_cityNodeResolver.TryGetCityByNodeId(nodeId, out _))
                    _cityNodeIds.Add(nodeId);
            }

            if (_cityNodeIds.Count < 2)
            {
                Debug.LogWarning("[HeadlessNpcLifeSimulator] Not enough city nodes to simulate NPCs.");
                return;
            }

            _spawner = new HeadlessNpcSpawner(
                _settings, _snapshot, _cityNodeResolver, _routeDecisionService, _rng, _cityNodeIds);
            _routeEnv = _spawner.RouteEnv;

            int count = Math.Max(1, _settings.AgentCount);
            for (int i = 0; i < count; i++)
                TrySpawnAgent();
        }

        public void AdvanceDay(int currentDay)
        {
            _currentDay = currentDay;
            var snapshot = new List<HeadlessCaravanAgent>(_agents);

            var economies = new List<NpcEconomyState>(snapshot.Count);
            var destinations = new List<string>(snapshot.Count);
            foreach (HeadlessCaravanAgent a in snapshot)
            {
                economies.Add(a.EconomyState);
                destinations.Add(a.DestinationNodeId ?? string.Empty);
            }

            var context = new NpcDayContext
            {
                Economies = economies,
                DestinationNodeIds = destinations,
                CurrentDay = currentDay,
                SuppliesSnapshot = new int[snapshot.Count],
                EstimateDaysToCity = i => _spawner.EstimateDaysToNearestCity(snapshot[i]),
                NextRandom = () => _rng.NextDouble()
            };

            _dayProcessor.ProcessDay(context);

            for (int i = 0; i < snapshot.Count; i++)
            {
                HeadlessCaravanAgent agent = snapshot[i];
                if (string.IsNullOrEmpty(agent.DestinationNodeId))
                    continue;
                if (!context.ForagedIndices.Contains(i))
                    agent.AdvanceDay();
            }

            foreach (int deadIdx in context.DeadIndices)
                KillAndRespawn(snapshot[deadIdx]);
        }

        private void HandleArrival(HeadlessCaravanAgent agent)
        {
            _statistics.RecordArrival();
            if (agent == null || _cityNodeIds.Count < 2)
                return;

            if (!_cityNodeResolver.TryGetCityByNodeId(agent.CurrentNodeId, out CityData city))
            {
                if (_spawner.TryChooseTarget(agent.CurrentNodeId, out string fallback))
                    agent.SetDestination(fallback, _snapshot);
                else
                    KillAndRespawn(agent);
                return;
            }

            var context = new NpcCityVisitContext
            {
                Economy = agent.EconomyState,
                City = city,
                CurrentNodeId = agent.CurrentNodeId,
                SpeedMetersPerDay = agent.SpeedMetersPerDay,
                CurrentDay = _currentDay,
                NextRandom = () => (float)_rng.NextDouble(),
                RouteEnvironment = _routeEnv
            };

            _visitProcessor.Process(context);

            if (!string.IsNullOrEmpty(context.NextTargetNodeId))
                agent.SetDestination(context.NextTargetNodeId, _snapshot);

            if (string.IsNullOrEmpty(agent.DestinationNodeId))
            {
                KillAndRespawn(agent);
                return;
            }

            NpcTrader.TradeExecutionStats total = context.SellStats + context.BuyStats;
            _statistics.RecordTrade(total);

            if (context.Traded)
                _statistics.RecordContract(context.ContractReward);

            _tradeLog.Add($"Day {_currentDay}: {agent.EconomyState.Name} traded at {city.Id}, money={agent.EconomyState.Money}");
            if (_tradeLog.Count > TradeLogCapacity)
                _tradeLog.RemoveAt(0);

            OnTradeCompleted?.Invoke();
        }

        private void KillAndRespawn(HeadlessCaravanAgent agent)
        {
            agent.OnArrived -= HandleArrival;
            _agents.Remove(agent);
            bool spawned = false;
            for (int attempt = 0; attempt < 3; attempt++)
            {
                if (TrySpawnAgent())
                {
                    spawned = true;
                    break;
                }
            }
            if (!spawned)
                Debug.LogWarning($"[HeadlessNpcLifeSimulator] KillAndRespawn: failed to respawn '{agent.EconomyState.Name}' after 3 attempts.");
        }

        private bool TrySpawnAgent()
        {
            HeadlessCaravanAgent agent = _spawner.TrySpawnAgent(_agents, _currentDay);
            if (agent == null) return false;

            agent.OnArrived += HandleArrival;
            _agents.Add(agent);
            return true;
        }
    }
}

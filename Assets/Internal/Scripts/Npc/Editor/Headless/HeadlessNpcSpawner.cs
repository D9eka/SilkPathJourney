using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Npc.Routing;
using Internal.Scripts.Road.Graph;
using UnityEngine;

namespace Internal.Scripts.Npc.Editor.Headless
{
    internal sealed class HeadlessNpcSpawner
    {
        private readonly NpcSimulationSettings _settings;
        private readonly RoadGraphSnapshot _snapshot;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly NpcRouteDecisionService _routeDecisionService;
        private readonly System.Random _rng;
        private readonly IReadOnlyList<string> _cityNodeIds;
        private readonly NpcSpawnRoller _spawnRoller;
        private readonly HeadlessRouteDecisionEnvironment _routeEnv;

        private int _totalSpawned;

        public HeadlessRouteDecisionEnvironment RouteEnv => _routeEnv;

        public HeadlessNpcSpawner(
            NpcSimulationSettings settings,
            RoadGraphSnapshot snapshot,
            ICityNodeResolver cityNodeResolver,
            NpcRouteDecisionService routeDecisionService,
            System.Random rng,
            IReadOnlyList<string> cityNodeIds)
        {
            _settings = settings;
            _snapshot = snapshot;
            _cityNodeResolver = cityNodeResolver;
            _routeDecisionService = routeDecisionService;
            _rng = rng;
            _cityNodeIds = cityNodeIds;
            _spawnRoller = new NpcSpawnRoller(settings);
            _routeEnv = new HeadlessRouteDecisionEnvironment(cityNodeIds, cityNodeResolver, snapshot, rng);
        }

        public HeadlessCaravanAgent TrySpawnAgent(IReadOnlyList<HeadlessCaravanAgent> agents, int currentDay)
        {
            if (!TryChooseSpawnStartNode(out string start))
                return null;

            float speedMin = Math.Min(_settings.SpeedRangeMetersPerDay.x, _settings.SpeedRangeMetersPerDay.y);
            float speedMax = Math.Max(_settings.SpeedRangeMetersPerDay.x, _settings.SpeedRangeMetersPerDay.y);
            float speed = speedMin + (float)_rng.NextDouble() * (speedMax - speedMin);

            NpcArchetype archetype = _spawnRoller.ChooseArchetypeToSpawn(arch => CountByArchetype(arch, agents));
            NpcExperienceLevel experience = _spawnRoller.RollExperience(
                (min, max) => _rng.Next(min, max));

            int money;
            float capacity;

            if (_settings.Archetypes != null && _settings.Archetypes.Count > 0)
            {
                NpcArchetypeDefinition archDef = _spawnRoller.FindArchetypeDef(archetype);
                money = _rng.Next(archDef.MoneyRange.x, archDef.MoneyRange.y + 1);
                capacity = archDef.CapacityRange.x + (float)_rng.NextDouble() * (archDef.CapacityRange.y - archDef.CapacityRange.x);
            }
            else
            {
                money = _rng.Next(_settings.MoneyRange.x, _settings.MoneyRange.y + 1);
                capacity = _settings.CapacityRange.x + (float)_rng.NextDouble() * (_settings.CapacityRange.y - _settings.CapacityRange.x);
            }

            string agentName = $"NPC_{_totalSpawned++}";

            NpcEconomyState economy = new NpcEconomyState(agentName, money, capacity)
            {
                Archetype = archetype,
                Experience = experience
            };

            if (!TryChoosePlannedTarget(start, economy, speed, currentDay, out string target))
                return null;

            int startSupplies = EstimateSuppliesForTrip(start, target, speed);
            InventoryStateMutator.AddItems(economy.Inventory, SuppliesItemId.Value, startSupplies);

            HeadlessCaravanAgent agent = new HeadlessCaravanAgent(economy, start, speed, 0, 0);
            agent.SetDestination(target, _snapshot);
            return agent;
        }

        private int CountByArchetype(NpcArchetype arch, IReadOnlyList<HeadlessCaravanAgent> agents)
        {
            int count = 0;
            foreach (HeadlessCaravanAgent a in agents)
                if (a.EconomyState.Archetype == arch) count++;
            return count;
        }

        private bool TryChooseSpawnStartNode(out string start)
        {
            start = null;
            if (_cityNodeIds.Count < 2)
                return false;

            start = _cityNodeIds[_rng.Next(_cityNodeIds.Count)];
            return !string.IsNullOrEmpty(start);
        }

        private bool TryChoosePlannedTarget(
            string currentNodeId,
            NpcEconomyState economy,
            float speed,
            int currentDay,
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
                    economy,
                    currentNodeId,
                    city.Id,
                    speed,
                    currentDay),
                _routeEnv,
                NextRandomValue);

            if (!routeDecision.HasTarget)
                return TryChooseTarget(currentNodeId, out target);

            target = routeDecision.TargetNodeId;
            return true;
        }

        public bool TryChooseTarget(string currentNodeId, out string target)
        {
            target = null;
            if (_cityNodeIds.Count < 2)
                return false;

            for (int attempt = 0; attempt < 5; attempt++)
            {
                string candidate = _cityNodeIds[_rng.Next(_cityNodeIds.Count)];
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
            float dist = _snapshot.GetDistance(fromNodeId, toNodeId);
            if (dist >= float.MaxValue || speed <= 0f)
                return _settings.StartingSupplies;

            int estimatedDays = Mathf.CeilToInt(dist / speed);
            return (estimatedDays + _settings.ExtraSuppliesDays) * _settings.SuppliesPerDay;
        }

        public float EstimateDaysToNearestCity(HeadlessCaravanAgent agent)
        {
            Vector3 pos = _snapshot.GetPosition(agent.CurrentNodeId);
            float minDistSqr = float.MaxValue;

            foreach (string cityNodeId in _cityNodeIds)
            {
                Vector3 cityPos = _snapshot.GetPosition(cityNodeId);
                float distSqr = (cityPos - pos).sqrMagnitude;
                if (distSqr < minDistSqr)
                    minDistSqr = distSqr;
            }

            if (minDistSqr >= float.MaxValue) return 1f;

            float euclidean = Mathf.Sqrt(minDistSqr);
            float roadDist = euclidean * _settings.RoadWindingFactor;
            return Mathf.Max(1f, roadDist / agent.SpeedMetersPerDay);
        }

        private float NextRandomValue() => (float)_rng.NextDouble();
    }
}

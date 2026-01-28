using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Nodes;

namespace Internal.Scripts.Npc.Lifecycle
{
    public sealed class NpcLifeSimulator : IInitializable, IDisposable
    {
        private readonly NpcSimulationSettings _settings;
        private readonly NpcFactory _factory;
        private readonly IRoadNodeLookup _nodeLookup;
        private readonly System.Random _random = new();

        private readonly List<RoadAgent> _agents = new();
        private List<string> _nodeIds;

        public NpcLifeSimulator(NpcSimulationSettings settings, NpcFactory factory, IRoadNodeLookup nodeLookup)
        {
            _settings = settings;
            _factory = factory;
            _nodeLookup = nodeLookup;
        }

        public void Initialize()
        {
            _nodeIds = new List<string>(_nodeLookup.Nodes.Keys);
            if (_nodeIds.Count < 2)
            {
                Debug.LogWarning("[NpcLifeSimulator] Not enough nodes to simulate NPCs.");
                return;
            }

            int count = Mathf.Max(1, _settings.AgentCount);
            for (int i = 0; i < count; i++)
            {
                //TrySpawnAgent();
            }
        }

        public void Dispose()
        {
            foreach (RoadAgent agent in _agents)
            {
                if (agent != null)
                    agent.OnArrived -= HandleArrival;
            }
            _agents.Clear();
        }

        private void TrySpawnAgent()
        {
            if (!TryChoosePrefab(out NpcView prefab) || !TryChooseTwoNodes(out string start, out string target))
            {
                Debug.LogWarning("[NpcLifeSimulator] Failed to spawn agent (prefab or nodes missing).");
                return;
            }

            Color color = ChooseColor();
            RoadAgentConfig config = BuildRandomConfig();

            RoadAgent agent = _factory.CreateFromPrefab(prefab, config, start, color);
            agent.OnArrived += HandleArrival;
            agent.SetDestination(target);
            _agents.Add(agent);

            Debug.Log($"[NpcLifeSimulator] Spawned agent '{prefab.name}' color {color} speed {config.SpeedMetersPerSecond:F1} from {start} to {target}");
        }

        private void HandleArrival(RoadAgent agent)
        {
            if (agent == null || _nodeIds.Count < 2)
                return;

            if (TryChooseTarget(agent.CurrentNodeId, out string target))
            {
                agent.SetDestination(target);
                Debug.Log($"[NpcLifeSimulator] Agent retargeted: {agent.CurrentNodeId} -> {target}");
            }
        }

        private bool TryChoosePrefab(out NpcView prefab)
        {
            prefab = null;
            if (_settings.Prefabs == null || _settings.Prefabs.Count == 0)
                return false;

            int idx = _random.Next(0, _settings.Prefabs.Count);
            prefab = _settings.Prefabs[idx];
            return prefab != null;
        }

        private Color ChooseColor()
        {
            if (_settings.AvailableColors != null && _settings.AvailableColors.Count > 0)
                return _settings.AvailableColors[_random.Next(0, _settings.AvailableColors.Count)];

            return Color.white;
        }

        private bool TryChooseTwoNodes(out string start, out string target)
        {
            start = null;
            target = null;

            if (_nodeIds == null || _nodeIds.Count < 2)
                return false;

            int a = _random.Next(0, _nodeIds.Count);
            int b;
            do
            {
                b = _random.Next(0, _nodeIds.Count);
            } while (b == a && _nodeIds.Count > 1);

            start = _nodeIds[a];
            target = _nodeIds[b];
            return start != target;
        }

        private bool TryChooseTarget(string currentNodeId, out string target)
        {
            target = null;
            if (_nodeIds == null || _nodeIds.Count < 2)
                return false;

            for (int attempt = 0; attempt < 5; attempt++)
            {
                string candidate = _nodeIds[_random.Next(0, _nodeIds.Count)];
                if (candidate != currentNodeId)
                {
                    target = candidate;
                    return true;
                }
            }
            return false;
        }

        private RoadAgentConfig BuildRandomConfig()
        {
            float min = Mathf.Min(_settings.SpeedRangeMetersPerSecond.x, _settings.SpeedRangeMetersPerSecond.y);
            float max = Mathf.Max(_settings.SpeedRangeMetersPerSecond.x, _settings.SpeedRangeMetersPerSecond.y);
            float speed = Mathf.Lerp(min, max, (float)_random.NextDouble());

            return new RoadAgentConfig
            {
                SpeedMetersPerSecond = speed,
                Lane = RoadLane.Right,
                LateralOffsetMeters = 0f
            };
        }
    }
}

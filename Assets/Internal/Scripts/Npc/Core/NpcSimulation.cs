using System;
using System.Collections.Generic;
using Internal.Scripts.Events;
using Internal.Scripts.World.State;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Npc.Core
{
    public sealed class NpcSimulation : ITickable, IDisposable
    {
        private const float MIN_GAP_METERS = 2f;

        private readonly List<RoadAgent> _agents = new();
        private readonly IWorldSimulationState _worldState;
        private readonly DayTracker _dayTracker;

        public NpcSimulation(IWorldSimulationState worldState, DayTracker dayTracker)
        {
            _worldState = worldState;
            _dayTracker = dayTracker;
        }

        public void Register(RoadAgent agent)
        {
            if (agent != null && !_agents.Contains(agent))
            {
                _agents.Add(agent);
                agent.Initialize();
            }
        }

        public void Unregister(RoadAgent agent)
        {
            if (agent != null)
            {
                _agents.Remove(agent);
                agent.Dispose();
            }
        }

        public void Tick()
        {
            if (!_worldState.IsActive && !_dayTracker.IsSkipping)
                return;

            for (int i = 0; i < _agents.Count; i++)
                _agents[i].SpeedMetersPerDay = _agents[i].BaseSpeedMetersPerDay;

            ApplySeparation();

            for (int i = 0; i < _agents.Count; i++)
                _agents[i].Tick();
        }

        private void ApplySeparation()
        {
            for (int i = 0; i < _agents.Count; i++)
            {
                RoadAgent a = _agents[i];
                if (!a.HasPath) continue;

                for (int j = 0; j < _agents.Count; j++)
                {
                    if (i == j) continue;
                    RoadAgent b = _agents[j];
                    if (!b.HasPath) continue;

                    if (a.CurrentFromNodeId != b.CurrentFromNodeId
                        || a.CurrentToNodeId != b.CurrentToNodeId)
                        continue;

                    float gap = b.DistanceOnSegment - a.DistanceOnSegment;
                    if (gap > 0f && gap < MIN_GAP_METERS)
                        a.SpeedMetersPerDay = Mathf.Min(a.SpeedMetersPerDay, b.SpeedMetersPerDay);
                }
            }
        }

        public void Dispose()
        {
            foreach (RoadAgent agent in _agents)
            {
                agent.Dispose();
            }
        }
    }
}

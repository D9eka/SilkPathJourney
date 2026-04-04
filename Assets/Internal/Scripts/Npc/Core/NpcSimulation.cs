using System;
using System.Collections.Generic;
using Internal.Scripts.Events;
using Internal.Scripts.World.State;
using Zenject;

namespace Internal.Scripts.Npc.Core
{
    public sealed class NpcSimulation : ITickable, IDisposable
    {
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
                _agents[i].Tick();
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

using System;
using System.Collections.Generic;
using Zenject;

namespace Internal.Scripts.Npc.Core
{
    public sealed class NpcSimulation : ITickable, IDisposable
    {
        private readonly List<RoadAgent> _agents = new();

        public void Register(RoadAgent agent)
        {
            if (agent != null && !_agents.Contains(agent))
            {
                _agents.Add(agent);
                agent.Initialize();
            }
        }

        public void Tick()
        {
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

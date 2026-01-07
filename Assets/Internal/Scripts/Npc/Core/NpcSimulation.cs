using System.Collections.Generic;
using Zenject;
using UnityEngine;

namespace Internal.Scripts.Npc.Core
{
    public sealed class NpcSimulation : ITickable
    {
        private readonly List<NpcAgent> _agents = new();

        public void Register(NpcAgent agent)
        {
            if (agent != null && !_agents.Contains(agent))
                _agents.Add(agent);
        }

        public void Tick()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < _agents.Count; i++)
                _agents[i].Tick(dt);
        }
    }
}

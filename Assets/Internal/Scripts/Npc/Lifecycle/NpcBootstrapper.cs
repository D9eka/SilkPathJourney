using System;
using UnityEngine;
using Zenject;
using Internal.Scripts.Npc.Core;

namespace Internal.Scripts.Npc.Lifecycle
{
    public sealed class NpcBootstrapper : IInitializable
    {
        private readonly NpcFactory _factory;
        private readonly NpcSpawnEntry[] _spawns;

        public NpcBootstrapper(NpcFactory factory, NpcSpawnEntry[] spawns)
        {
            _factory = factory;
            _spawns = spawns ?? Array.Empty<NpcSpawnEntry>();
        }

        public void Initialize()
        {
            foreach (NpcSpawnEntry entry in _spawns)
            {
                if (entry?.View == null)
                    continue;

                string start = Choose(entry.StartNodeId, entry.View.DefaultStartNodeId);
                string destination = Choose(entry.DestinationNodeId, entry.View.DefaultDestinationNodeId);

                RoadAgent agent = _factory.Create(entry.View, entry.Config ?? new NpcConfig(), start);

                if (entry.AutoStart && !string.IsNullOrWhiteSpace(destination))
                    agent.SetDestination(destination);
            }
        }

        private static string Choose(string preferred, string fallback) =>
            !string.IsNullOrWhiteSpace(preferred) ? preferred : fallback;
    }
}

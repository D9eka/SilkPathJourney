using Internal.Scripts.Npc.Core.NextSegmentProvider;
using Internal.Scripts.Npc.Movement;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.Npc.Core
{
    public sealed class NpcFactory
    {
        private readonly IRoadPathFinder _pathFinder;
        private readonly IRoadNodeLookup _nodeLookup;
        private readonly IRoadNetwork _network;
        private readonly RoadSamplerCache _samplerCache;
        private readonly NpcSimulation _simulation;
        private readonly RoadPoseSampler _poseSampler;

        public NpcFactory(IRoadPathFinder pathFinder, IRoadNodeLookup nodeLookup, IRoadNetwork network, 
            RoadSamplerCache samplerCache, NpcSimulation simulation,  RoadPoseSampler poseSampler)
        {
            _pathFinder = pathFinder;
            _nodeLookup = nodeLookup;
            _network = network;
            _samplerCache = samplerCache;
            _simulation = simulation;
            _poseSampler = poseSampler;
        }

        public RoadAgent Create(NpcView view, NpcConfig config, string startNodeId)
        {
            RoadPathCursor cursor = new RoadPathCursor(new SegmentMover(_network, _samplerCache, _poseSampler), new NpcNextSegmentProvider());
            RoadAgent agent = new RoadAgent(view, config, _pathFinder, _nodeLookup, cursor, startNodeId);
            _simulation.Register(agent);
            return agent;
        }

        public RoadAgent CreateFromPrefab(NpcView prefab, NpcConfig config, string startNodeId, Color color)
        {
            NpcView instance = Object.Instantiate(prefab);
            instance.ApplyColor(color);
            return Create(instance, config, startNodeId);
        }
    }
}

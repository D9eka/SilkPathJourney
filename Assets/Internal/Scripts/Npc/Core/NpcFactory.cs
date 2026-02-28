using Internal.Scripts.Npc.Movement;
using Internal.Scripts.Npc.NextSegment;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using Internal.Scripts.World.State;
using UnityEngine;

namespace Internal.Scripts.Npc.Core
{
    public sealed class NpcFactory
    {
        private readonly IRoadPathFinder _pathFinder;
        private readonly IRoadNetwork _network;
        private readonly RoadSamplerCache _samplerCache;
        private readonly NpcSimulation _simulation;
        private readonly RoadPoseSampler _poseSampler;
        private readonly IGameDayDeltaProvider _gameDayDeltaProvider;

        public NpcFactory(IRoadPathFinder pathFinder, IRoadNetwork network,
            RoadSamplerCache samplerCache, NpcSimulation simulation, RoadPoseSampler poseSampler,
            IGameDayDeltaProvider gameDayDeltaProvider)
        {
            _pathFinder = pathFinder;
            _network = network;
            _samplerCache = samplerCache;
            _simulation = simulation;
            _poseSampler = poseSampler;
            _gameDayDeltaProvider = gameDayDeltaProvider;
        }

        public RoadAgent Create(NpcView view, RoadAgentConfig config, string startNodeId)
        {
            RoadPathCursor cursor = new RoadPathCursor(_network,
                new SegmentMover(_network, _samplerCache, _poseSampler),
                new NpcNextSegmentProvider(_pathFinder));
            RoadAgent agent = new RoadAgent(view, config, cursor, _gameDayDeltaProvider, startNodeId);
            _simulation.Register(agent);
            return agent;
        }

        public RoadAgent CreateFromPrefab(NpcView prefab, RoadAgentConfig config, string startNodeId, Color color)
        {
            NpcView instance = Object.Instantiate(prefab);
            instance.ApplyColor(color);
            return Create(instance, config, startNodeId);
        }
    }
}

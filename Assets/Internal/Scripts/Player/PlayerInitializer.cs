using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.Core.NextSegmentProvider;
using Internal.Scripts.Npc.Movement;
using Internal.Scripts.Player.UI.Arrow.JunctionBalancer;
using Internal.Scripts.Road.Path;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Player
{
    public class PlayerInitializer : IInitializable, ITickable
    {
        private readonly RoadAgentView _view;
        private readonly RoadAgentConfig _config;
        private readonly IRoadPathFinder _pathFinder;
        private readonly SegmentMover _segmentMover;
        private readonly INextSegmentProvider _nextSegmentProvider;
        private readonly IArrowJunctionBalancer _arrowJunctionBalancer;
        private readonly string _startNodeId;
        private readonly string _endNodeId;
        
        private RoadAgent _agent;

        public PlayerInitializer(RoadAgentView view, RoadAgentConfig config, IRoadPathFinder pathFinder, 
            SegmentMover segmentMover, INextSegmentProvider nextSegmentProvider, 
            IArrowJunctionBalancer arrowJunctionBalancer, string startNodeId, string endNodeId)
        {
            _view = view;
            _config = config;
            _pathFinder = pathFinder;
            _segmentMover = segmentMover;
            _nextSegmentProvider = nextSegmentProvider;
            _arrowJunctionBalancer = arrowJunctionBalancer;
            _startNodeId = startNodeId;
            _endNodeId = endNodeId;
        }

        public void Initialize()
        {
            _agent = new RoadAgent(_view,  _config, _pathFinder, 
                new RoadPathCursor(_segmentMover, _nextSegmentProvider), _startNodeId);
            _agent.Initialize();
            _agent.SetDestination(_endNodeId);
            _arrowJunctionBalancer.Initialize(_agent);
        }
        
        public void Tick()
        {
            _agent.Tick(Time.deltaTime);
        }
    }
}
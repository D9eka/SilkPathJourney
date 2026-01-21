using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.Movement;
using Internal.Scripts.Npc.NextSegment;
using Internal.Scripts.Player.UI.Arrow.JunctionBalancer;
using Internal.Scripts.Road.Graph;
using Zenject;

namespace Internal.Scripts.Player
{
    public class PlayerInitializer : IInitializable
    {
        private readonly RoadAgentView _view;
        private readonly RoadAgentConfig _config;
        private readonly IRoadNetwork _roadNetwork;
        private readonly SegmentMover _segmentMover;
        private readonly INextSegmentProvider _nextSegmentProvider;
        private readonly IArrowJunctionBalancer _arrowJunctionBalancer;
        private readonly PlayerController _playerController;
        private readonly string _startNodeId;

        public PlayerInitializer(RoadAgentView view, RoadAgentConfig config, 
            IRoadNetwork roadNetwork, SegmentMover segmentMover, 
            INextSegmentProvider nextSegmentProvider, IArrowJunctionBalancer arrowJunctionBalancer, 
            PlayerController playerController, string startNodeId)
        {
            _view = view;
            _config = config;
            _roadNetwork = roadNetwork;
            _segmentMover = segmentMover;
            _nextSegmentProvider = nextSegmentProvider;
            _arrowJunctionBalancer = arrowJunctionBalancer;
            _playerController = playerController;
            _startNodeId = startNodeId;
        }

        public void Initialize()
        {
            RoadAgent agent = new RoadAgent(_view,  _config, 
                new RoadPathCursor(_roadNetwork, _segmentMover, _nextSegmentProvider), _startNodeId);
            agent.Initialize();
            _arrowJunctionBalancer.Initialize(agent);
            _playerController.Initialize(agent);
        }
    }
}
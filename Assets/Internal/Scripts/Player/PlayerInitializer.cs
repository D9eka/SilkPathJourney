using System;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.Movement;
using Internal.Scripts.Npc.NextSegment;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Save;
using Internal.Scripts.UI.Arrow.JunctionBalancer;
using Internal.Scripts.UI.StartMovement;
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
        private readonly PlayerConfig _playerConfig;
        private readonly SaveRepository _saveRepository;
        private readonly IPlayerStartMovement _playerStartMovement;

        public PlayerInitializer(RoadAgentView view, RoadAgentConfig config, 
            IRoadNetwork roadNetwork, SegmentMover segmentMover, 
            INextSegmentProvider nextSegmentProvider, IArrowJunctionBalancer arrowJunctionBalancer, 
            PlayerController playerController, PlayerConfig playerConfig, SaveRepository saveRepository,
            IPlayerStartMovement playerStartMovement)
        {
            _view = view;
            _config = config;
            _roadNetwork = roadNetwork;
            _segmentMover = segmentMover;
            _nextSegmentProvider = nextSegmentProvider;
            _arrowJunctionBalancer = arrowJunctionBalancer;
            _playerController = playerController;
            _playerConfig = playerConfig;
            _saveRepository = saveRepository;
            _playerStartMovement = playerStartMovement;
        }

        public void Initialize()
        {
            string startNodeId = ResolveStartNodeId();

            RoadAgent agent = new RoadAgent(_view,  _config, 
                new RoadPathCursor(_roadNetwork, _segmentMover, _nextSegmentProvider), startNodeId);
            agent.Initialize();
            _arrowJunctionBalancer.Initialize(agent);
            _playerController.Initialize(agent);

            string destinationNodeId = ResolveDestinationNodeId();
            if (!string.IsNullOrWhiteSpace(destinationNodeId) && destinationNodeId != startNodeId)
            {
                agent.SetDestination(destinationNodeId);
                _playerStartMovement.SetStartButtonEnabled(false);
            }
            else
            {
                _playerStartMovement.SetStartButtonEnabled(true);
            }
        }

        private string ResolveStartNodeId()
        {
            string savedNodeId = _saveRepository?.Data?.Player?.CurrentNodeId;
            if (!string.IsNullOrWhiteSpace(savedNodeId))
                return savedNodeId;

            if (!string.IsNullOrWhiteSpace(_playerConfig.StartNodeId))
                return _playerConfig.StartNodeId;

            throw new NullReferenceException("No start node id found in saves or player config");
        }

        private string ResolveDestinationNodeId()
        {
            return _saveRepository?.Data?.Player?.DestinationNodeId ?? string.Empty;
        }
    }
}

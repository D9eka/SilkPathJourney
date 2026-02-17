using System;
using Internal.Scripts.Player;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using Zenject;

namespace Internal.Scripts.UI.PathVisualization
{
    public class PathVisualizationController : IInitializable, IDisposable
    {
        private readonly PlayerController _playerController;
        private readonly IRoadPathFinder _pathFinder;
        private readonly RoadNetwork _roadNetwork;
        private readonly IPathVisualizationService _visualizationService;

        public PathVisualizationController(
            PlayerController playerController,
            IRoadPathFinder pathFinder,
            RoadNetwork roadNetwork,
            IPathVisualizationService visualizationService)
        {
            _playerController = playerController;
            _pathFinder = pathFinder;
            _roadNetwork = roadNetwork;
            _visualizationService = visualizationService;
        }

        public void Initialize()
        {
            _playerController.OnDestinationChanged += OnDestinationChanged;
            UpdatePathVisualization();
        }

        public void Dispose()
        {
            _playerController.OnDestinationChanged -= OnDestinationChanged;
        }

        private void OnDestinationChanged(string destinationNodeId)
        {
            UpdatePathVisualization();
        }

        private void UpdatePathVisualization()
        {
            string currentNode = _playerController.CurrentNodeId;
            string destinationNode = _playerController.DestinationNodeId;

            if (string.IsNullOrEmpty(currentNode) || string.IsNullOrEmpty(destinationNode))
            {
                _visualizationService.HidePath();
                return;
            }

            RoadPath path = _pathFinder.FindPath(currentNode, destinationNode);

            if (path != null && path.IsValid)
            {
                _visualizationService.ShowPath(path);
            }
            else
            {
                _visualizationService.HidePath();
                UnityEngine.Debug.LogWarning($"Path not found from {currentNode} to {destinationNode}");
            }
        }
    }
}

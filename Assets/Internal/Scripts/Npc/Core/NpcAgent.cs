using System;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.Npc.Core
{
    public sealed class NpcAgent
    {
        public event Action<NpcAgent> OnArrived;

        private readonly NpcView _view;
        private readonly NpcConfig _config;
        private readonly IRoadPathFinder _pathFinder;
        private readonly IRoadNodeLookup _nodeLookup;
        private readonly RoadPathCursor _cursor;

        private string _currentNodeId;
        private string _destinationNodeId;

        public string CurrentNodeId => _currentNodeId;
        public string DestinationNodeId => _destinationNodeId;
        public bool HasPath => !_cursor.IsEmpty && !_cursor.IsComplete;

        public NpcAgent(NpcView view, NpcConfig config, IRoadPathFinder pathFinder, IRoadNodeLookup nodeLookup, RoadPathCursor cursor, string startNodeId)
        {
            _view = view;
            _config = config ?? new NpcConfig();
            _pathFinder = pathFinder;
            _nodeLookup = nodeLookup;
            _cursor = cursor;
            _currentNodeId = startNodeId;

            SnapToNode(_currentNodeId);
        }

        public void SetDestination(string destinationNodeId)
        {
            if (string.IsNullOrWhiteSpace(destinationNodeId))
                return;

            if (_currentNodeId == destinationNodeId)
                return;

            RoadPath path = _pathFinder.FindPath(_currentNodeId, destinationNodeId);
            if (!path.IsValid)
            {
                Debug.LogWarning($"[NpcAgent] Path not found {_currentNodeId} -> {destinationNodeId}.");
                return;
            }

            _destinationNodeId = destinationNodeId;
            _cursor.SetPath(path, _config.Lane, _config.LateralOffsetMeters);
        }

        public void Tick(float deltaTime)
        {
            if (_cursor.IsEmpty || deltaTime <= 0f)
                return;

            _cursor.Advance(_config.SpeedMetersPerSecond * deltaTime);
            RoadPose pose = _cursor.CurrentPose;
            _view.SetPose(pose.Position, pose.Forward);

            if (_cursor.IsComplete && !string.IsNullOrEmpty(_destinationNodeId))
            {
                _currentNodeId = _destinationNodeId;
                _destinationNodeId = null;
                OnArrived?.Invoke(this);
            }
        }

        private void SnapToNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                return;

            Vector3? pos = _nodeLookup.GetPosition(nodeId);
            if (pos.HasValue)
                _view.SetPose(pos.Value, _view.VisualRoot.forward);
        }
    }
}

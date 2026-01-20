using System;
using Internal.Scripts.Npc.Movement;
using Internal.Scripts.Road.Path;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Npc.Core
{
    public sealed class RoadAgent : IInitializable, IDisposable
    {
        public event Action<RoadAgent> OnArrived;

        private readonly RoadAgentView _view;
        private readonly RoadAgentConfig _config;
        private readonly IRoadPathFinder _pathFinder;
        private readonly RoadPathCursor _cursor;

        private string _currentNodeId;
        private string _destinationNodeId;

        public string CurrentNodeId => _currentNodeId;
        public string DestinationNodeId => _destinationNodeId;
        public bool HasPath => !_cursor.IsEmpty;
        public RoadPose CurrentPose => _cursor.CurrentPose;

        public RoadAgent(RoadAgentView view, RoadAgentConfig config, 
            IRoadPathFinder pathFinder, RoadPathCursor cursor, string startNodeId)
        {
            _view = view;
            _config = config ?? new RoadAgentConfig();
            _pathFinder = pathFinder;
            _cursor = cursor;
            _currentNodeId = startNodeId;
        }
        
        public void Initialize()
        {
            _cursor.Initialize();
        }
        
        public void Dispose()
        {
            _cursor.Dispose();
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
            if (_cursor.IsEmpty && !string.IsNullOrEmpty(_destinationNodeId))
            {
                _currentNodeId = _destinationNodeId;
                _destinationNodeId = null;
                OnArrived?.Invoke(this);
                return;
            }
            
            _cursor.Advance(_config.SpeedMetersPerSecond * deltaTime);
            RoadPose pose = _cursor.CurrentPose;
            _view.SetPose(pose.Position, pose.Forward);
        }
    }
}

using System;
using Internal.Scripts.Npc.Movement;
using Internal.Scripts.Road.Path;
using Internal.Scripts.World.State;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Npc.Core
{
    public sealed class RoadAgent : IInitializable, IDisposable
    {
        public event Action<RoadAgent> OnArrived;

        private readonly RoadAgentView _view;
        private readonly RoadAgentConfig _config;
        private readonly RoadPathCursor _cursor;
        private readonly IGameDayDeltaProvider _gameDayDeltaProvider;

        private string _currentNodeId;
        private string _destinationNodeId;

        public string CurrentNodeId => _currentNodeId;
        public string DestinationNodeId => _destinationNodeId;
        public bool HasPath => !_cursor.IsEmpty;
        public RoadPose CurrentPose => _cursor.CurrentPose;
        public float SpeedMetersPerDay { get; set; }
        public float Weight { get; set; }

        public RoadAgent(RoadAgentView view, RoadAgentConfig config,
            RoadPathCursor cursor, IGameDayDeltaProvider gameDayDeltaProvider, string startNodeId)
        {
            _view = view;
            _config = config ?? new RoadAgentConfig();
            _cursor = cursor;
            _gameDayDeltaProvider = gameDayDeltaProvider;
            _currentNodeId = startNodeId;
            SpeedMetersPerDay = _config.SpeedMetersPerDay;
        }
        
        public void Initialize()
        {
            _cursor.Initialize(_currentNodeId);
            RoadPose pose = _cursor.CurrentPose;
            _view.SetPose(pose.Position, pose.Forward);
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

            _destinationNodeId = destinationNodeId;
            _cursor.SetDestination(_currentNodeId, _destinationNodeId, 
                _config.Lane, _config.LateralOffsetMeters);
        }

        public void CancelDestination(string currentNodeId)
        {
            if (!string.IsNullOrWhiteSpace(currentNodeId))
                _currentNodeId = currentNodeId;

            _destinationNodeId = null;
            _cursor.CancelPath();
        }

        public void Tick()
        {
            if (_cursor.IsEmpty)
            {
                if (_cursor.HasArrived && !string.IsNullOrEmpty(_destinationNodeId))
                {
                    _currentNodeId = _destinationNodeId;
                    _destinationNodeId = null;
                    OnArrived?.Invoke(this);
                }
                else if (!string.IsNullOrEmpty(_destinationNodeId))
                {
                    Debug.LogWarning($"[RoadAgent] Path to '{_destinationNodeId}' failed. Resetting to idle.");
                    _destinationNodeId = null;
                }
                return;
            }

            float dayDelta = _gameDayDeltaProvider.GetFrameDayDelta();
            _cursor.AdvanceByDaySpeed(SpeedMetersPerDay, dayDelta);

            RoadPose pose = _cursor.CurrentPose;
            _view.SetPose(pose.Position, pose.Forward);
        }
    }
}

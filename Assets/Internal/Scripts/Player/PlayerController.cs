using System;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Player.StartMovement;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Player
{
    public class PlayerController : ITickable, IDisposable, IPlayerStateProvider, IPlayerStateEvents, IPlayerMovementControl
    {
        private readonly IPlayerStartMovement _playerStartMovement;
        
        private RoadAgent _roadAgent;
        private string _lastDestinationId;

        public string CurrentNodeId => _roadAgent?.CurrentNodeId ?? string.Empty;
        public string DestinationNodeId => _roadAgent?.DestinationNodeId ?? string.Empty;

        public event Action<string> OnCurrentNodeChanged;
        public event Action<string> OnDestinationChanged;

        public PlayerState State
        {
            get
            {
                if (_playerStartMovement.IsChoosingTarget)
                    return PlayerState.SelectingTarget;

                if (_roadAgent != null && (_roadAgent.HasPath || 
                    !string.IsNullOrEmpty(_roadAgent.DestinationNodeId)))
                    return PlayerState.Moving;

                return PlayerState.Idle;
            }
        }

        public PlayerController(IPlayerStartMovement playerStartMovement)
        {
            _playerStartMovement = playerStartMovement;
        }

        public void Initialize(RoadAgent roadAgent)
        {
            _roadAgent = roadAgent;
            _lastDestinationId = DestinationNodeId ?? string.Empty;
            _playerStartMovement.SetCurrentPlayerNode(_roadAgent.CurrentNodeId);
            
            _playerStartMovement.OnChooseNode += StartPath;
            _roadAgent.OnArrived += EndPath;
        }
        
        public void Tick()
        {
            _roadAgent.Tick(Time.deltaTime);
        }

        public void Dispose()
        {
            _playerStartMovement.OnChooseNode -= StartPath;
            _roadAgent.OnArrived -= EndPath;
        }

        private void StartPath(string nodeId)
        {
            _roadAgent.SetDestination(nodeId);
            NotifyDestinationChanged();
        }

        private void EndPath(RoadAgent roadAgent)
        {
            _playerStartMovement.SetCurrentPlayerNode(_roadAgent.CurrentNodeId);
            OnCurrentNodeChanged?.Invoke(_roadAgent.CurrentNodeId);
            NotifyDestinationChanged();
        }

        public void CancelDestinationAtNode(string nodeId)
        {
            if (_roadAgent == null)
                return;

            if (string.IsNullOrWhiteSpace(nodeId))
                nodeId = _roadAgent.CurrentNodeId;

            _roadAgent.CancelDestination(nodeId);
            _playerStartMovement.SetCurrentPlayerNode(nodeId);
            OnCurrentNodeChanged?.Invoke(nodeId);
            NotifyDestinationChanged();
        }

        private void NotifyDestinationChanged()
        {
            string destinationId = DestinationNodeId ?? string.Empty;
            if (_lastDestinationId == destinationId)
                return;

            _lastDestinationId = destinationId;
            OnDestinationChanged?.Invoke(destinationId);
        }

    }
}

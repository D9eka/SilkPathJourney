using System;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Player.UI.StartMovement;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Player
{
    public class PlayerController : ITickable, IDisposable, IPlayerStateProvider
    {
        private readonly IPlayerStartMovement _playerStartMovement;
        
        private RoadAgent _roadAgent;

        public string CurrentNodeId => _roadAgent?.CurrentNodeId ?? string.Empty;

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
        }

        private void EndPath(RoadAgent roadAgent)
        {
            _playerStartMovement.FinishPath();
            _playerStartMovement.SetCurrentPlayerNode(_roadAgent.CurrentNodeId);
        }

    }
}

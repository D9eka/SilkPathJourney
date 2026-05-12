using System;
using Internal.Scripts.Camera.Move;
using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.Input;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Player;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Camera.Follow
{
    public class CameraFollowService : ICameraFollowService, IInitializable, ILateTickable, IDisposable
    {
        private readonly ICameraMover _mover;
        private readonly ICameraZoomer _zoomer;
        private readonly ICameraInput _inputManager;
        private readonly CameraSceneSettings _settings;
        private readonly CameraBounds _bounds;
        private readonly RoadAgentView _roadAgentView;
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly IPlayerStateEvents _playerStateEvents;

        private bool _isFollowing;
        private bool _isTransitioning;

        public bool IsFollowing => _isFollowing || _isTransitioning;
        public event Action<bool> OnFollowStateChanged;

        public CameraFollowService(
            ICameraMover mover,
            ICameraZoomer zoomer,
            ICameraInput inputManager,
            CameraSceneSettings settings,
            CameraBounds bounds,
            RoadAgentView roadAgentView,
            IPlayerStateProvider playerStateProvider,
            IPlayerStateEvents playerStateEvents)
        {
            _mover = mover;
            _zoomer = zoomer;
            _inputManager = inputManager;
            _settings = settings;
            _bounds = bounds;
            _roadAgentView = roadAgentView;
            _playerStateProvider = playerStateProvider;
            _playerStateEvents = playerStateEvents;
        }

        public void Initialize()
        {
            _inputManager.OnChangePosition += OnInput;
            _playerStateEvents.OnCurrentNodeChanged += OnNodeChanged;

            SnapToPlayer();
            StartFollowing();
        }

        private void SnapToPlayer()
        {
            if (_roadAgentView == null || _roadAgentView.VisualRoot == null)
                return;
            Vector3 p = _roadAgentView.VisualRoot.position;
            Vector2 clamped = _bounds.Clamp(new Vector2(p.x, p.z));
            _mover.ApplyPosition(clamped);
        }

        public void LateTick()
        {
            if (!_isFollowing && !_isTransitioning)
                return;

            if (_roadAgentView == null || _roadAgentView.VisualRoot == null)
                return;

            Vector3 playerPos = _roadAgentView.VisualRoot.position;
            Vector2 targetXZ = new Vector2(playerPos.x, playerPos.z);
            Vector2 clamped = _bounds.Clamp(targetXZ);

            _mover.ApplyPosition(clamped);
        }

        public void StartFollowing()
        {
            if (_isFollowing || _isTransitioning)
                return;

            _isTransitioning = true;
            _mover.SuspendLateTick = true;
            _mover.ResetMovement();

            float followZoom = _settings.FollowZoomSize;
            float duration = _settings.FollowTransitionDuration;
            _zoomer.ZoomTo(followZoom, duration, OnTransitionComplete);
        }

        public void StopFollowing()
        {
            bool wasActive = _isFollowing || _isTransitioning;
            _isFollowing = false;
            _isTransitioning = false;
            _mover.SuspendLateTick = false;

            if (wasActive)
                OnFollowStateChanged?.Invoke(false);
        }

        public void Dispose()
        {
            _inputManager.OnChangePosition -= OnInput;
            _playerStateEvents.OnCurrentNodeChanged -= OnNodeChanged;
        }

        private void OnTransitionComplete()
        {
            if (!_isTransitioning)
                return;

            _isTransitioning = false;
            _isFollowing = true;
            OnFollowStateChanged?.Invoke(true);
        }

        private void OnInput(Vector2 delta)
        {
            if ((_isFollowing || _isTransitioning) && delta != Vector2.zero)
                StopFollowing();
        }

        private void OnNodeChanged(string nodeId)
        {
            if (_isFollowing && _playerStateProvider.State != PlayerState.Moving)
                StopFollowing();
        }
    }
}

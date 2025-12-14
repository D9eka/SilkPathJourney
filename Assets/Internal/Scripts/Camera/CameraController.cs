using System;
using Internal.Scripts.Camera.AutoFit;
using Internal.Scripts.Camera.Move;
using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.Road.Follower;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Camera
{
    public class CameraController : IFixedTickable
    {
        private readonly ICameraMover _mover;
        private readonly ICameraZoomer _zoomer;
        private readonly ICameraAutoFitter _autoFitter;
        
        private RoadFollowerView _roadFollowerView;

        public CameraController(ICameraMover mover, ICameraZoomer zoomer, ICameraAutoFitter autoFitter)
        {
            _mover = mover;
            _zoomer = zoomer;
            _autoFitter = autoFitter;
        }

        public void MoveCamera(Vector2 position, Action onComplete = null) => _mover.MoveTo(position, onComplete);
        public void ZoomCamera(float size, Action onComplete = null) => _zoomer.ZoomTo(size, onComplete);

        public void FocusOnObjects(Transform[] targets, Action onComplete = null)
        {
            _roadFollowerView = null;
            _autoFitter.FocusOnObjects(targets, onComplete);
        }

        public void FocusOnRoadFollower(RoadFollowerView roadFollowerView)
        {
            _roadFollowerView = roadFollowerView;
        }

        public void FixedTick()
        {
            if (_roadFollowerView == null) return;
            _mover.MoveTo(_roadFollowerView.transform.position, 0);
        }
    }
}
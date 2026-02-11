using System;
using Internal.Scripts.Camera.AutoFit;
using Internal.Scripts.Camera.Move;
using Internal.Scripts.Camera.Zoom;
using UnityEngine;

namespace Internal.Scripts.Camera
{
    public class CameraController
    {
        private readonly ICameraMover _mover;
        private readonly ICameraZoomer _zoomer;
        private readonly ICameraAutoFitter _autoFitter;

        public CameraController(ICameraMover mover, ICameraZoomer zoomer, ICameraAutoFitter autoFitter)
        {
            _mover = mover;
            _zoomer = zoomer;
            _autoFitter = autoFitter;
        }

        public float CurrentZoomSize => _zoomer.Size;

        public void MoveCamera(Vector2 position, Action onComplete = null) => _mover.MoveTo(position, onComplete);
        public void MoveCamera(Vector2 position, float duration, Action onComplete = null) => _mover.MoveTo(position, duration, onComplete);
        public void ZoomCamera(float size, Action onComplete = null) => _zoomer.ZoomTo(size, onComplete);
        public void ZoomCamera(float size, float duration, Action onComplete = null) => _zoomer.ZoomTo(size, duration, onComplete);

        public void FocusOnObjects(Transform[] targets, Action onComplete = null)
        {
            _autoFitter.FocusOnObjects(targets, onComplete);
        }
    }
}
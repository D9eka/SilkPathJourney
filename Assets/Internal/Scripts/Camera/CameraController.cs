using System;
using Internal.Scripts.Camera.AutoFit;
using Internal.Scripts.Camera.Move;
using Internal.Scripts.Camera.Tilt;
using Internal.Scripts.Camera.Zoom;
using UnityEngine;

namespace Internal.Scripts.Camera
{
    public class CameraController
    {
        private readonly ICameraMover _mover;
        private readonly ICameraZoomer _zoomer;
        private readonly ICameraTilter _tilter;
        private readonly ICameraAutoFitter _autoFitter;

        public CameraController(ICameraMover mover, ICameraZoomer zoomer, ICameraTilter tilter, ICameraAutoFitter autoFitter)
        {
            _mover = mover;
            _zoomer = zoomer;
            _tilter = tilter;
            _autoFitter = autoFitter;
        }

        public float CurrentZoomSize => _zoomer.Size;
        public float CurrentTiltAngle => _tilter.CurrentAngle;

        public void MoveCamera(Vector2 position, Action onComplete = null) => _mover.MoveTo(position, onComplete);
        public void MoveCamera(Vector2 position, float duration, Action onComplete = null) => _mover.MoveTo(position, duration, onComplete);
        public void ZoomCamera(float size, Action onComplete = null) => _zoomer.ZoomTo(size, onComplete);
        public void ZoomCamera(float size, float duration, Action onComplete = null) => _zoomer.ZoomTo(size, duration, onComplete);
        public void TiltCamera(float angle, float duration, Action onComplete = null) => _tilter.TiltTo(angle, duration, onComplete);

        public void FocusOnObjects(Transform[] targets, Action onComplete = null)
        {
            _autoFitter.FocusOnObjects(targets, onComplete);
        }
    }
}

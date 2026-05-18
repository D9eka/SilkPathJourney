using System;
using Internal.Scripts.Camera.AutoFit;
using Internal.Scripts.Camera.Follow;
using Internal.Scripts.Camera.Move;
using Internal.Scripts.Camera.Tilt;
using Internal.Scripts.Camera.Zoom;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Camera
{
    public class CameraController : ILateTickable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly ICameraMover _mover;
        private readonly ICameraZoomer _zoomer;
        private readonly ICameraTilter _tilter;
        private readonly ICameraAutoFitter _autoFitter;
        private readonly ICameraFollowService _followService;
        private readonly CameraBounds _bounds;

        private bool _focusActive;
        private float _focusElapsed;
        private float _focusDuration;
        private Vector2 _focusStartTarget;
        private Vector2 _focusEndTarget;
        private float _focusStartY;
        private float _focusEndY;
        private float _focusAngle;
        private float _focusYRot;
        private Action _focusOnComplete;

        public CameraController(UnityEngine.Camera camera, ICameraMover mover, ICameraZoomer zoomer,
            ICameraTilter tilter, ICameraAutoFitter autoFitter, ICameraFollowService followService,
            CameraBounds bounds)
        {
            _camera = camera;
            _mover = mover;
            _zoomer = zoomer;
            _tilter = tilter;
            _autoFitter = autoFitter;
            _followService = followService;
            _bounds = bounds;
        }

        public Transform CameraTransform => _camera.transform;
        public float CurrentZoomSize => _zoomer.Size;
        public float CurrentTiltAngle => _tilter.CurrentAngle;

        public bool IsFollowingPlayer => _followService.IsFollowing;

        public event Action<bool> OnFollowStateChanged
        {
            add => _followService.OnFollowStateChanged += value;
            remove => _followService.OnFollowStateChanged -= value;
        }

        public void MoveCamera(Vector2 position, Action onComplete = null) => _mover.MoveTo(position, onComplete);
        public void MoveCamera(Vector2 position, float duration, Action onComplete = null) => _mover.MoveTo(position, duration, onComplete);
        public void ZoomCamera(float size, Action onComplete = null) => _zoomer.ZoomTo(size, onComplete);
        public void ZoomCamera(float size, float duration, Action onComplete = null) => _zoomer.ZoomTo(size, duration, onComplete);
        public void TiltCamera(float angle, float duration, Action onComplete = null) => _tilter.TiltTo(angle, duration, onComplete);

        public void FollowPlayer() => _followService.StartFollowing();
        public void UnfollowPlayer() => _followService.StopFollowing();

        public void SuspendInput()
        {
            _mover.SuspendLateTick = true;
            _mover.ResetMovement();
        }

        public void ResumeInput()
        {
            _mover.SuspendLateTick = false;
        }

        public void FocusOnObjects(Transform[] targets, Action onComplete = null)
        {
            _autoFitter.FocusOnObjects(targets, onComplete);
        }

        public void CancelFocus()
        {
            if (!_focusActive) return;
            _focusActive = false;
            _mover.SuspendLateTick = false;
            _focusOnComplete = null;
        }

        public void FocusOn(Vector2 worldTarget, float zoomSize, float duration, Action onComplete = null)
        {
            _zoomer.Stop();
            _mover.SuspendLateTick = true;
            _mover.ResetMovement();

            _focusStartTarget = _camera.transform.GetWorldTarget();
            _focusStartY = _camera.transform.position.y;
            _focusEndY = _zoomer.SizeToY(zoomSize);
            _focusAngle = _camera.transform.eulerAngles.x;
            _focusYRot = _camera.transform.eulerAngles.y;
            _focusEndTarget = _bounds.ClampForCameraY(worldTarget, _focusEndY);

            _focusDuration = Mathf.Max(duration, 0.001f);
            _focusElapsed = 0f;
            _focusOnComplete = onComplete;
            _focusActive = true;
        }

        public void LateTick()
        {
            if (!_focusActive) return;

            _focusElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_focusElapsed / _focusDuration);
            float eased = EaseOutSine(t);

            float y = Mathf.Lerp(_focusStartY, _focusEndY, eased);
            Vector2 target = Vector2.Lerp(_focusStartTarget, _focusEndTarget, eased);
            float zOffset = CameraExtensions.CalculateZOffset(y, _focusAngle, _focusYRot);
            _camera.transform.position = new Vector3(target.x, y, target.y + zOffset);

            if (t >= 1f)
            {
                _focusActive = false;
                _mover.SuspendLateTick = false;
                Action cb = _focusOnComplete;
                _focusOnComplete = null;
                cb?.Invoke();
            }
        }

        private static float EaseOutSine(float t) => Mathf.Sin(t * Mathf.PI * 0.5f);
    }
}

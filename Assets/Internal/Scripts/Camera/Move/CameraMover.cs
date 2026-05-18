using System;
using Internal.Scripts.Camera.Tilt;
using Internal.Scripts.Input;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Camera.Move
{
    public class CameraMover : ICameraMover, IInitializable, ILateTickable, IDisposable
    {
        private const float AnimSpeedUnitsPerSec = 10f;

        private readonly UnityEngine.Camera _camera;
        private readonly ICameraInput _inputManager;
        private readonly ICameraTilter _tilter;
        private readonly CameraBounds _bounds;
        private readonly CameraSceneSettings _settings;

        private Vector2 _moveDelta;

        private Vector2? _animTarget;
        private Vector2 _animStart;
        private bool _animStartPending;
        private float _animElapsed;
        private float _animDuration;
        private Action _animCallback;

        public bool SuspendLateTick { get; set; }

        public CameraMover(UnityEngine.Camera camera, ICameraInput inputManager, ICameraTilter tilter,
            CameraBounds bounds, CameraSceneSettings settings)
        {
            _camera = camera;
            _inputManager = inputManager;
            _tilter = tilter;
            _bounds = bounds;
            _settings = settings;
        }

        public void Initialize()
        {
            _inputManager.OnChangePosition += ChangePosition;
            ApplyPosition(_bounds.Center);
        }

        public void LateTick()
        {
            if (SuspendLateTick) return;

            if (_animTarget.HasValue)
            {
                if (_animStartPending)
                {
                    _animStart = _camera.transform.GetWorldTarget();
                    _animStartPending = false;
                }

                _animElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(_animElapsed / _animDuration);
                float eased = EaseInOutSine(t);
                Vector2 current = Vector2.Lerp(_animStart, _animTarget.Value, eased);
                ApplyPosition(_bounds.Clamp(current));

                if (t >= 1f)
                {
                    Action cb = _animCallback;
                    _animTarget = null;
                    _animCallback = null;
                    cb?.Invoke();
                }
                return;
            }

            if (_tilter.IsAnimating) return;

            float yFactor = Mathf.Min(_camera.transform.position.y, _settings.MaxMoveSpeedHeight);
            float speed = _settings.MoveSensitivity * yFactor * Time.deltaTime;
            _camera.transform.position += new Vector3(_moveDelta.x, 0, _moveDelta.y) * speed;

            Vector2 worldTarget = _camera.transform.GetWorldTarget();
            Vector2 clamped = _bounds.Clamp(worldTarget);
            if (worldTarget != clamped)
                ApplyPosition(clamped);
        }

        public void Dispose()
        {
            _inputManager.OnChangePosition -= ChangePosition;
        }

        public void MoveTo(Vector2 worldPosition, Action onComplete = null)
        {
            Vector2 currentWorld = _camera.transform.GetWorldTarget();
            float distance = Vector2.Distance(currentWorld, worldPosition);
            float duration = distance / AnimSpeedUnitsPerSec;
            MoveTo(worldPosition, duration, onComplete);
        }

        public void MoveTo(Vector2 worldPosition, float duration, Action onComplete = null)
        {
            _animTarget = worldPosition;
            _animStartPending = true;
            _animElapsed = 0f;
            _animDuration = Mathf.Max(duration, 0.001f);
            _animCallback = onComplete;
        }

        public void ResetMovement()
        {
            _moveDelta = Vector2.zero;
            _animTarget = null;
            _animStartPending = false;
            _animCallback = null;
        }

        public void ApplyPosition(Vector2 worldTarget)
        {
            float zOffset = _camera.transform.GetZOffset();
            float currentY = _camera.transform.position.y;
            _camera.transform.position = new Vector3(worldTarget.x, currentY, worldTarget.y + zOffset);
        }

        private static float EaseInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;

        private void ChangePosition(Vector2 delta)
        {
            _moveDelta = delta;
        }
    }
}

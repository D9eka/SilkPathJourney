using System;
using DG.Tweening;
using Internal.Scripts.Camera.Tilt;
using Internal.Scripts.Input;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Camera.Move
{
    public class CameraMover : ICameraMover, IInitializable, ILateTickable, IDisposable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly InputRouter _inputManager;
        private readonly ICameraTilter _tilter;
        private readonly CameraBounds _bounds;
        private readonly CameraSceneSettings _settings;

        private Vector2 _moveDelta;
        private Tween _tweenX;
        private Tween _tweenZ;

        public bool SuspendLateTick { get; set; }

        public CameraMover(UnityEngine.Camera camera, InputRouter inputManager, ICameraTilter tilter,
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
            _inputManager.OnChangeCameraPosition += ChangePosition;

            float zOffset = _camera.transform.GetZOffset();
            Vector3 pos = _camera.transform.position;
            _camera.transform.position = new Vector3(
                _bounds.Center.x, pos.y, _bounds.Center.y + zOffset);
        }

        public void LateTick()
        {
            if (SuspendLateTick) return;

            if ((_tweenX != null && _tweenX.IsActive() && _tweenX.IsPlaying()) ||
                _tilter.IsAnimating)
                return;

            float yFactor = Mathf.Min(_camera.transform.position.y, _settings.MaxMoveSpeedHeight);
            float speed = _settings.MoveSensitivity * yFactor * Time.deltaTime;
            _camera.transform.position += new Vector3(_moveDelta.x, 0, _moveDelta.y) * speed;

            Vector2 worldTarget = _camera.transform.GetWorldTarget();
            Vector2 clamped = _bounds.Clamp(worldTarget);
            if (worldTarget != clamped)
            {
                float zOffset = _camera.transform.GetZOffset();
                Vector3 pos = _camera.transform.position;
                _camera.transform.position = new Vector3(clamped.x, pos.y, clamped.y + zOffset);
            }
        }

        public void Dispose()
        {
            _tweenX?.Kill();
            _tweenZ?.Kill();
            _inputManager.OnChangeCameraPosition -= ChangePosition;
        }

        public void MoveTo(Vector2 worldPosition, Action onComplete = null)
        {
            Vector2 currentWorld = _camera.transform.GetWorldTarget();
            float dx = worldPosition.x - currentWorld.x;
            float dz = worldPosition.y - currentWorld.y;
            float distance = Mathf.Sqrt(dx * dx + dz * dz);
            float duration = distance / 10f;
            MoveTo(worldPosition, duration, onComplete);
        }

        public void MoveTo(Vector2 worldPosition, float duration, Action onComplete = null)
        {
            worldPosition = _bounds.Clamp(worldPosition);
            float zOffset = _camera.transform.GetZOffset();

            _tweenX?.Kill();
            _tweenZ?.Kill();

            int completed = 0;
            void Check() { if (++completed >= 2) onComplete?.Invoke(); }
            _tweenX = _camera.transform.DOMoveX(worldPosition.x, duration).OnComplete(Check);
            _tweenZ = _camera.transform.DOMoveZ(worldPosition.y + zOffset, duration).OnComplete(Check);
        }

        public void ResetMovement()
        {
            _moveDelta = Vector2.zero;
        }

        private void ChangePosition(Vector2 delta)
        {
            _moveDelta = delta;
        }
    }
}

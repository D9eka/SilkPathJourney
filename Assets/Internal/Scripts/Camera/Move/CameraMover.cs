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
        private readonly InputManager _inputManager;
        private readonly ICameraTilter _tilter;

        private Vector2 _moveDelta;
        private Tween _tweenX;
        private Tween _tweenZ;

        public CameraMover(UnityEngine.Camera camera, InputManager inputManager, ICameraTilter tilter)
        {
            _camera = camera;
            _inputManager = inputManager;
            _tilter = tilter;
        }

        public void Initialize()
        {
            _inputManager.OnChangeCameraPosition += ChangePosition;
        }

        public void LateTick()
        {
            if ((_tweenX != null && _tweenX.IsActive() && _tweenX.IsPlaying()) ||
                _tilter.IsAnimating)
                return;

            _camera.transform.position += new Vector3(_moveDelta.x, 0, _moveDelta.y);
        }

        public void Dispose()
        {
            _tweenX?.Kill();
            _tweenZ?.Kill();
            _inputManager.OnChangeCameraPosition -= ChangePosition;
        }

        public void MoveTo(Vector2 worldPosition, Action onComplete = null)
        {
            Vector2 currentWorld = GetCurrentWorldTarget();
            float dx = worldPosition.x - currentWorld.x;
            float dz = worldPosition.y - currentWorld.y;
            float distance = Mathf.Sqrt(dx * dx + dz * dz);
            float duration = distance / 10f;
            MoveTo(worldPosition, duration, onComplete);
        }

        public void MoveTo(Vector2 worldPosition, float duration, Action onComplete = null)
        {
            float zOffset = CalculateZOffset();

            _tweenX?.Kill();
            _tweenZ?.Kill();

            int completed = 0;
            void Check() { if (++completed >= 2) onComplete?.Invoke(); }
            _tweenX = _camera.transform.DOMoveX(worldPosition.x, duration).OnComplete(Check);
            _tweenZ = _camera.transform.DOMoveZ(worldPosition.y + zOffset, duration).OnComplete(Check);
        }

        private Vector2 GetCurrentWorldTarget()
        {
            Vector3 pos = _camera.transform.position;
            float zOffset = CalculateZOffset();
            return new Vector2(pos.x, pos.z - zOffset);
        }

        private float CalculateZOffset()
        {
            Vector3 forward = _camera.transform.forward;
            if (Mathf.Abs(forward.y) < 0.001f) return 0f;
            return _camera.transform.position.y * forward.z / forward.y;
        }

        private void ChangePosition(Vector2 delta)
        {
            _moveDelta = delta;
        }
    }
}

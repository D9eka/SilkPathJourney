using System;
using DG.Tweening;
using Internal.Scripts.Input;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Camera.Move
{
    public class CameraMover : ICameraMover, IInitializable, ILateTickable, IDisposable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly InputManager _inputManager;

        private Vector2 _moveDelta;
        private Tween _tweenX;
        private Tween _tweenZ;

        public CameraMover(UnityEngine.Camera camera, InputManager inputManager)
        {
            _camera = camera;
            _inputManager = inputManager;
        }

        public void Initialize()
        {
            _inputManager.OnChangeCameraPosition += ChangePosition;
        }

        public void LateTick()
        {
            if (_tweenX != null && _tweenX.IsActive() && _tweenX.IsPlaying())
                return;

            _camera.transform.position += new Vector3(_moveDelta.x, 0, _moveDelta.y);
        }

        public void Dispose()
        {
            _tweenX?.Kill();
            _tweenZ?.Kill();
            _inputManager.OnChangeCameraPosition -= ChangePosition;
        }

        public void MoveTo(Vector2 position, Action onComplete = null)
        {
            Vector3 current = _camera.transform.position;
            float dx = position.x - current.x;
            float dz = position.y - current.z;
            float distance = Mathf.Sqrt(dx * dx + dz * dz);
            float duration = distance / 10f;
            MoveTo(position, duration, onComplete);
        }

        public void MoveTo(Vector2 position, float duration, Action onComplete = null)
        {
            _tweenX?.Kill();
            _tweenZ?.Kill();

            int completed = 0;
            void Check() { if (++completed >= 2) onComplete?.Invoke(); }
            _tweenX = _camera.transform.DOMoveX(position.x, duration).OnComplete(Check);
            _tweenZ = _camera.transform.DOMoveZ(position.y, duration).OnComplete(Check);
        }

        private void ChangePosition(Vector2 delta)
        {
            _moveDelta = delta;
        }
    }
}

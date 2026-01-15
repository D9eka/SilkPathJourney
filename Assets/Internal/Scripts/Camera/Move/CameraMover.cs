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
            _camera.transform.position += new Vector3(_moveDelta.x, 0, _moveDelta.y);
        }
        
        public void Dispose()
        {
            _inputManager.OnChangeCameraPosition -= ChangePosition;
        }

        public void MoveTo(Vector2 position, Action onComplete = null)
        {
            float duration = Vector3.Distance(_camera.transform.position, GetVector3Position(position)) / 10f;
            MoveTo(position, duration, onComplete);
        }

        public void MoveTo(Vector2 position, float duration, Action onComplete = null)
        {
            _camera.transform.DOMove(GetVector3Position(position), duration)
                .OnComplete(() => onComplete?.Invoke());
        }

        private Vector3 GetVector3Position(Vector2 position)
        {
            return new Vector3(position.x, position.y, _camera.transform.position.z);
        }
        
        private void ChangePosition(Vector2 delta)
        {
            _moveDelta = delta;
        }
    }
}
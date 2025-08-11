using System;
using DG.Tweening;
using UnityEngine;

namespace Internal.Scripts.Camera.Move
{
    public class CameraMover : ICameraMover
    {
        private readonly UnityEngine.Camera _camera;

        public CameraMover(UnityEngine.Camera camera)
        {
            _camera = camera;
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
    }
}
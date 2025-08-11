using System;
using DG.Tweening;
using UnityEngine;

namespace Internal.Scripts.Camera.Zoom
{
    public class CameraZoomer : ICameraZoomer
    {
        private readonly UnityEngine.Camera _camera;

        public CameraZoomer(UnityEngine.Camera camera)
        {
            _camera = camera;
        }

        public void ZoomTo(float size, Action onComplete = null)
        {
            float duration = Mathf.Abs(_camera.orthographicSize - size) / 10f;
            ZoomTo(size, duration, onComplete);
        }

        public void ZoomTo(float size, float duration, Action onComplete = null)
        {
            _camera.DOOrthoSize(size, duration)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}
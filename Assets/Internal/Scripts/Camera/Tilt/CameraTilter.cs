using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Camera.Tilt
{
    public class CameraTilter : ICameraTilter, IInitializable, IDisposable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly CameraSceneSettings _settings;
        private Tween _tween;

        public float CurrentAngle => _camera.transform.eulerAngles.x;
        public bool IsAnimating => _tween != null && _tween.IsActive() && _tween.IsPlaying();

        public CameraTilter(UnityEngine.Camera camera, CameraSceneSettings settings)
        {
            _camera = camera;
            _settings = settings;
        }

        public void Initialize()
        {
            Vector3 euler = _camera.transform.eulerAngles;
            _camera.transform.eulerAngles = new Vector3(_settings.StrategicTiltAngle, euler.y, euler.z);
        }

        public void TiltTo(float angle, float duration, Action onComplete = null)
        {
            _tween?.Kill();
            Vector3 euler = _camera.transform.eulerAngles;
            _tween = _camera.transform.DORotate(new Vector3(angle, euler.y, euler.z), duration)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void Dispose()
        {
            _tween?.Kill();
        }
    }
}

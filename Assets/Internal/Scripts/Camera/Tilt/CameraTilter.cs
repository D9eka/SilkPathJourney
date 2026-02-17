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
            float startAngle = _camera.transform.eulerAngles.x;
            float yRot = _camera.transform.eulerAngles.y;
            Vector2 worldTarget = GetWorldTarget();

            _tween = DOVirtual.Float(0f, 1f, duration, t =>
            {
                float currentAngle = Mathf.Lerp(startAngle, angle, t);
                _camera.transform.eulerAngles = new Vector3(currentAngle, yRot, 0f);

                float y = _camera.transform.position.y;
                float zOffset = CalculateZOffset(y, currentAngle, yRot);
                _camera.transform.position = new Vector3(
                    _camera.transform.position.x, y, worldTarget.y + zOffset);
            }).SetEase(Ease.InOutSine).OnComplete(() => onComplete?.Invoke());
        }

        public void Dispose()
        {
            _tween?.Kill();
        }

        private Vector2 GetWorldTarget()
        {
            Vector3 pos = _camera.transform.position;
            Vector3 forward = _camera.transform.forward;
            if (Mathf.Abs(forward.y) < 0.001f)
                return new Vector2(pos.x, pos.z);
            float zOffset = pos.y * forward.z / forward.y;
            return new Vector2(pos.x, pos.z - zOffset);
        }

        private float CalculateZOffset(float cameraY, float xAngleDeg, float yAngleDeg)
        {
            float xRad = xAngleDeg * Mathf.Deg2Rad;
            float yRad = yAngleDeg * Mathf.Deg2Rad;
            float forwardY = -Mathf.Sin(xRad);
            if (Mathf.Abs(forwardY) < 0.001f) return 0f;
            float forwardZ = Mathf.Cos(xRad) * Mathf.Cos(yRad);
            return cameraY * forwardZ / forwardY;
        }
    }
}

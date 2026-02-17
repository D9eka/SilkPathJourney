using System;
using DG.Tweening;
using Internal.Scripts.Camera;
using Internal.Scripts.Camera.Zoom;
using UnityEngine;

namespace Internal.Scripts.Economy.Cities
{
    public class CityViewAnimator
    {
        private readonly CameraBounds _cameraBounds;
        private readonly CameraZoomerData _zoomerData;

        private Tween _transitionTween;

        public CityViewAnimator(CameraBounds cameraBounds, CameraZoomerData zoomerData)
        {
            _cameraBounds = cameraBounds;
            _zoomerData = zoomerData;
        }

        public void AnimateMove(Vector2 start, Vector2 end, float y, float angle,
            float yRotation, float duration, Action onComplete)
        {
            _transitionTween?.Kill();
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            float zOffset = CalculateZOffset(y, angle, yRotation);

            _transitionTween = DOVirtual.Float(0f, 1f, duration, t =>
            {
                float x = Mathf.Lerp(start.x, end.x, t);
                float wz = Mathf.Lerp(start.y, end.y, t);
                Vector2 clamped = _cameraBounds.Clamp(new Vector2(x, wz));
                cam.transform.position = new Vector3(clamped.x, y, clamped.y + zOffset);
            }).SetEase(Ease.InOutSine).OnComplete(() => onComplete?.Invoke());
        }

        public void AnimateZoomTilt(Vector2 worldTarget, float startSize, float endSize,
            float startAngle, float endAngle, float yRotation, float duration, Action onComplete)
        {
            _transitionTween?.Kill();
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            float startY = cam.transform.position.y;
            float endY = SizeToY(endSize);

            _transitionTween = DOVirtual.Float(0f, 1f, duration, t =>
            {
                float angle = Mathf.Lerp(startAngle, endAngle, t);
                float y = Mathf.Lerp(startY, endY, t);
                float zOffset = CalculateZOffset(y, angle, yRotation);
                cam.transform.position = new Vector3(worldTarget.x, y, worldTarget.y + zOffset);
                cam.transform.eulerAngles = new Vector3(angle, yRotation, 0f);
                Vector2 clamped = _cameraBounds.Clamp(worldTarget);
                if (clamped != worldTarget)
                    cam.transform.position = new Vector3(clamped.x, y, clamped.y + zOffset);
            }).SetEase(Ease.InOutSine).OnComplete(() => onComplete?.Invoke());
        }

        public void Kill()
        {
            _transitionTween?.Kill();
        }

        public Vector2 GetWorldTarget(UnityEngine.Camera cam)
        {
            Vector3 forward = cam.transform.forward;
            Vector3 pos = cam.transform.position;
            if (Mathf.Abs(forward.y) < 0.001f)
                return new Vector2(pos.x, pos.z);
            float zOffset = pos.y * forward.z / forward.y;
            return new Vector2(pos.x, pos.z - zOffset);
        }

        public float CalculateZOffset(float cameraY, float xAngleDeg, float yAngleDeg)
        {
            float xRad = xAngleDeg * Mathf.Deg2Rad;
            float yRad = yAngleDeg * Mathf.Deg2Rad;
            float forwardY = -Mathf.Sin(xRad);
            if (Mathf.Abs(forwardY) < 0.001f) return 0f;
            float forwardZ = Mathf.Cos(xRad) * Mathf.Cos(yRad);
            return cameraY * forwardZ / forwardY;
        }

        public float SizeToY(float size)
        {
            return _zoomerData.BaseYPosition +
                   (size - _zoomerData.BaseSizeValue) / _zoomerData.ScaleFactor;
        }

        public float YToSize(float y)
        {
            return _zoomerData.BaseSizeValue +
                   (y - _zoomerData.BaseYPosition) * _zoomerData.ScaleFactor;
        }
    }
}

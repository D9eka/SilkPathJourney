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
            float zOffset = CameraExtensions.CalculateZOffset(y, angle, yRotation);

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
            float endY = _zoomerData.SizeToY(endSize);

            _transitionTween = DOVirtual.Float(0f, 1f, duration, t =>
            {
                float angle = Mathf.Lerp(startAngle, endAngle, t);
                float y = Mathf.Lerp(startY, endY, t);
                float zOffset = CameraExtensions.CalculateZOffset(y, angle, yRotation);
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
            => cam.transform.GetWorldTarget();

        public float SizeToY(float size)
            => _zoomerData.SizeToY(size);

        public float YToSize(float y)
            => _zoomerData.YToSize(y);
    }
}

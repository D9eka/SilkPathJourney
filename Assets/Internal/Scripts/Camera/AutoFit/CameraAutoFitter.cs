using System;
using Internal.Scripts.Camera.Move;
using Internal.Scripts.Camera.Zoom;
using UnityEngine;

namespace Internal.Scripts.Camera.AutoFit
{
    public class CameraAutoFitter : ICameraAutoFitter
    {
        private readonly UnityEngine.Camera _camera;
        private readonly ICameraMover _mover;
        private readonly ICameraZoomer _zoomer;
        private readonly float _padding;

        public CameraAutoFitter(UnityEngine.Camera camera, ICameraMover mover, ICameraZoomer zoomer, float padding = 10f)
        {
            _camera = camera;
            _mover = mover;
            _zoomer = zoomer;
            _padding = padding;
        }

        public void FocusOnObjects(Transform[] targets, Action onComplete)
        {
            if (targets == null || targets.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            if (!TryCalculateBounds(targets, out Bounds bounds))
            {
                onComplete?.Invoke();
                return;
            }

            Vector3 center = CalculateCenter(bounds);
            float finalSize = CalculateCameraSize(bounds);

            (float moveDuration, float zoomDuration) = CalculateDurations(center, finalSize);

            int completedCount = 0;
            void CheckComplete()
            {
                completedCount++;
                if (completedCount >= 2)
                    onComplete?.Invoke();
            }

            _zoomer.ZoomTo(finalSize, zoomDuration, CheckComplete);
            _mover.MoveTo(center, moveDuration, CheckComplete);
        }

        private bool TryCalculateBounds(Transform[] targets, out Bounds bounds)
        {
            bool hasBounds = false;
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            foreach (Transform target in targets)
            {
                if (target == null)
                {
                    continue;
                }

                Collider2D[] colliders = target.GetComponentsInChildren<Collider2D>(true);
                foreach (Collider2D collider in colliders)
                    UpdateMinMax(collider.bounds, ref minX, ref maxX, ref minY, ref maxY, ref hasBounds);

                SpriteRenderer[] sprites = target.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (SpriteRenderer spriteRenderer in sprites)
                    UpdateMinMax(spriteRenderer.bounds, ref minX, ref maxX, ref minY, ref maxY, ref hasBounds);

                if (!hasBounds)
                {
                    Vector3 position = target.position;
                    minX = Mathf.Min(minX, position.x);
                    maxX = Mathf.Max(maxX, position.x);
                    minY = Mathf.Min(minY, position.y);
                    maxY = Mathf.Max(maxY, position.y);
                    hasBounds = true;
                }
            }

            if (hasBounds)
            {
                bounds = new Bounds
                {
                    min = new Vector3(minX, minY),
                    max = new Vector3(maxX, maxY)
                };
                return true;
            }

            bounds = default;
            return false;
        }

        private Vector3 CalculateCenter(Bounds bounds)
        {
            return new Vector3(
                (bounds.min.x + bounds.max.x) * 0.5f,
                (bounds.min.y + bounds.max.y) * 0.5f,
                _camera.transform.position.z
            );
        }

        private float CalculateCameraSize(Bounds bounds)
        {
            float width = (bounds.max.x - bounds.min.x) + _padding;
            float height = (bounds.max.y - bounds.min.y) + _padding;
            float sizeByWidth = width / _camera.aspect * 0.5f;
            float sizeByHeight = height * 0.5f;
            return Mathf.Max(sizeByWidth, sizeByHeight);
        }

        private (float moveDuration, float zoomDuration) CalculateDurations(Vector3 targetPos, float finalSize)
        {
            float distance = Vector3.Distance(_camera.transform.position, targetPos);
            float zoomDelta = Mathf.Abs(_camera.orthographicSize - finalSize);
            float moveDuration = Mathf.Clamp(distance / 10f, 0.2f, 2.5f);
            float zoomDuration = Mathf.Clamp(zoomDelta / 10f, 0.2f, 2.5f);
            return (moveDuration, zoomDuration);
        }

        private void UpdateMinMax(Bounds b, ref float minX, ref float maxX, ref float minY, ref float maxY, ref bool hasBounds)
        {
            minX = Mathf.Min(minX, b.min.x);
            maxX = Mathf.Max(maxX, b.max.x);
            minY = Mathf.Min(minY, b.min.y);
            maxY = Mathf.Max(maxY, b.max.y);
            hasBounds = true;
        }
    }
}

using System;
using DG.Tweening;
using Internal.Scripts.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace Internal.Scripts.Camera.Zoom
{
    public class CameraZoomer : ICameraZoomer, IInitializable, IDisposable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly InputManager _inputManager;
        private readonly CameraZoomerData _cameraZoomerData;

        private Tween _tweenY;

        public float Size => _cameraZoomerData.BaseSizeValue +
            (_camera.transform.position.y - _cameraZoomerData.BaseYPosition) *
            _cameraZoomerData.ScaleFactor;

        public CameraZoomer(UnityEngine.Camera camera, InputManager inputManager, CameraZoomerData cameraZoomerData)
        {
            _camera = camera;
            _inputManager = inputManager;
            _cameraZoomerData = cameraZoomerData;
        }
        
        public void Initialize()
        {
            _inputManager.OnChangeCameraSize += ChangeSize;
        }
        public void Dispose()
        {
            _tweenY?.Kill();
            _inputManager.OnChangeCameraSize -= ChangeSize;
        }

        public void ZoomTo(float size, Action onComplete = null)
        {
            ZoomTo(size, null, onComplete);
        }

        public void ZoomTo(float size, Vector3? targetWorldPos, Action onComplete = null)
        {
            float zoomDelta = Mathf.Abs(Size - size);
            float currentY = Mathf.Abs(_camera.transform.position.y);

            float speedMultiplier = currentY / Mathf.Abs(_cameraZoomerData.BaseYPosition);
            speedMultiplier = Mathf.Max(speedMultiplier, 0.5f);

            float duration = zoomDelta / (_cameraZoomerData.ZoomSpeed * speedMultiplier);

            ZoomTo(size, targetWorldPos, duration, onComplete);
        }

        public void ZoomTo(float size, float duration, Action onComplete = null)
        {
            ZoomTo(size, null, duration, onComplete);
        }

        public void ZoomTo(float size, Vector3? targetWorldPos, float duration, Action onComplete = null)
        {
            float currentY = _camera.transform.position.y;
            float targetY = _cameraZoomerData.BaseYPosition +
                (size - _cameraZoomerData.BaseSizeValue) / _cameraZoomerData.ScaleFactor;

            Vector3 currentPos = _camera.transform.position;

            if (targetWorldPos.HasValue && _cameraZoomerData.EnableZoomToCursor)
            {
                float zoomRatio = targetY / currentY;
                float offsetScale = 1f - zoomRatio;

                Vector2 currentXZ = new Vector2(currentPos.x, currentPos.z);
                Vector2 targetXZ = new Vector2(targetWorldPos.Value.x, targetWorldPos.Value.z);
                Vector2 newXZ = currentXZ + (targetXZ - currentXZ) * offsetScale;

                _camera.transform.position = new Vector3(newXZ.x, currentPos.y, newXZ.y);
            }

            _tweenY?.Kill();

            _tweenY = _camera.transform.DOMoveY(targetY, duration)
                .OnComplete(() => onComplete?.Invoke());
        }

        private void ChangeSize(float sizeDelta)
        {
            if (Mathf.Approximately(sizeDelta, 0f)) return;

            float newSize = Size + sizeDelta * _cameraZoomerData.Sensitivity;
            newSize = Mathf.Clamp(newSize, _cameraZoomerData.MinValue, _cameraZoomerData.MaxValue);

            Vector3? mouseWorldPos = null;
            if (_cameraZoomerData.EnableZoomToCursor)
            {
                mouseWorldPos = TryGetMouseWorldPosition();
            }

            ZoomTo(newSize, mouseWorldPos);
        }

        private Vector3? TryGetMouseWorldPosition()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return null;

            if (Mouse.current == null)
                return null;

            Vector2 screenPos = Mouse.current.position.ReadValue();

            Plane xzPlane = new Plane(Vector3.up, new Vector3(0, _camera.transform.position.y, 0));
            Ray ray = _camera.ScreenPointToRay(screenPos);

            if (xzPlane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            return null;
        }
    }
}
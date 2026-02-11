using System;
using DG.Tweening;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Input;
using Internal.Scripts.Player;
using Internal.Scripts.Road.Nodes;
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
        private readonly CameraBounds _bounds;
        private readonly CameraSceneSettings _settings;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly IRoadNodeLookup _nodeLookup;

        private Tween _tweenY;

        public float Size => _cameraZoomerData.BaseSizeValue +
            (_camera.transform.position.y - _cameraZoomerData.BaseYPosition) *
            _cameraZoomerData.ScaleFactor;

        public CameraZoomer(
            UnityEngine.Camera camera,
            InputManager inputManager,
            CameraZoomerData cameraZoomerData,
            CameraBounds bounds,
            CameraSceneSettings settings,
            ICityNodeResolver cityNodeResolver,
            IPlayerStateProvider playerStateProvider,
            IRoadNodeLookup nodeLookup)
        {
            _camera = camera;
            _inputManager = inputManager;
            _cameraZoomerData = cameraZoomerData;
            _bounds = bounds;
            _settings = settings;
            _cityNodeResolver = cityNodeResolver;
            _playerStateProvider = playerStateProvider;
            _nodeLookup = nodeLookup;
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

            ClampPositionToBounds();

            _tweenY?.Kill();

            _tweenY = _camera.transform.DOMoveY(targetY, duration)
                .OnComplete(() => onComplete?.Invoke());
        }

        private void ChangeSize(float sizeDelta)
        {
            if (Mathf.Approximately(sizeDelta, 0f)) return;

            float newSize = Size + sizeDelta * _cameraZoomerData.Sensitivity;
            newSize = Mathf.Clamp(newSize, _cameraZoomerData.MinValue, _cameraZoomerData.MaxValue);

            if (_settings.EnableDetailSceneLoading)
            {
                float thresholdSize = YToSize(_settings.DetailSceneLoadThreshold);
                if (newSize < thresholdSize && !HasDetailSceneAtCurrentPosition())
                    newSize = thresholdSize;
            }

            Vector3? mouseWorldPos = null;
            if (_cameraZoomerData.EnableZoomToCursor)
            {
                mouseWorldPos = TryGetMouseWorldPosition();
            }

            ZoomTo(newSize, mouseWorldPos);
        }

        private float YToSize(float y)
        {
            return _cameraZoomerData.BaseSizeValue +
                (y - _cameraZoomerData.BaseYPosition) * _cameraZoomerData.ScaleFactor;
        }

        private bool HasDetailSceneAtCurrentPosition()
        {
            string nodeId = _playerStateProvider.CurrentNodeId;
            if (string.IsNullOrEmpty(nodeId)) return false;
            if (!_cityNodeResolver.TryGetCityByNodeId(nodeId, out CityData city)) return false;
            if (city.DetailScene == null || string.IsNullOrEmpty(city.DetailScene.SceneName)) return false;

            Vector3? nodePos = _nodeLookup.GetPosition(city.NodeId);
            if (!nodePos.HasValue) return false;

            Vector2 cityXZ = new Vector2(nodePos.Value.x, nodePos.Value.z);
            Vector2 cameraXZ = GetCameraWorldTarget();
            float maxDistance = city.DetailScene.CameraSize > 0
                ? city.DetailScene.CameraSize
                : _settings.DefaultCityDetailZoomSize;

            return Vector2.Distance(cameraXZ, cityXZ) <= maxDistance;
        }

        private Vector2 GetCameraWorldTarget()
        {
            Vector3 pos = _camera.transform.position;
            Vector3 forward = _camera.transform.forward;
            float zOffset = Mathf.Abs(forward.y) < 0.001f ? 0f : pos.y * forward.z / forward.y;
            return new Vector2(pos.x, pos.z - zOffset);
        }

        private void ClampPositionToBounds()
        {
            Vector2 worldTarget = GetCameraWorldTarget();
            Vector2 clamped = _bounds.Clamp(worldTarget);
            if (worldTarget != clamped)
            {
                Vector3 pos = _camera.transform.position;
                Vector3 forward = _camera.transform.forward;
                float zOffset = Mathf.Abs(forward.y) < 0.001f ? 0f : pos.y * forward.z / forward.y;
                _camera.transform.position = new Vector3(clamped.x, pos.y, clamped.y + zOffset);
            }
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
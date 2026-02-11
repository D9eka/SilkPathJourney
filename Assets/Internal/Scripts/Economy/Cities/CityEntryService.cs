using System;
using DG.Tweening;
using Internal.Scripts.Camera;
using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.Player;
using Internal.Scripts.Road.Nodes;
using UnityEngine;

namespace Internal.Scripts.Economy.Cities
{
    public interface ICityEntryService
    {
        bool IsInCityView { get; }
        CityData CurrentCity { get; }
        bool CanEnterCity(CityData city);
        void EnterCity(CityData city, Action onComplete = null);
        void ExitCity(Action onComplete = null);
        event Action<CityData> OnCityEntered;
        event Action OnCityExited;
    }

    public class CityEntryService : ICityEntryService
    {
        private readonly CameraController _cameraController;
        private readonly CameraSceneLoader _cameraSceneLoader;
        private readonly DetailSceneLoader _detailSceneLoader;
        private readonly CameraSceneSettings _settings;
        private readonly CameraZoomerData _zoomerData;
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly IRoadNodeLookup _nodeLookup;

        private CityData _currentCity;
        private bool _isTransitioning;
        private Vector2 _previousWorldTarget;
        private float _previousCameraSize;
        private float _cameraYRotation;
        private Tween _transitionTween;

        public bool IsInCityView => _currentCity != null;
        public CityData CurrentCity => _currentCity;

        public event Action<CityData> OnCityEntered;
        public event Action OnCityExited;

        public CityEntryService(
            CameraController cameraController,
            CameraSceneLoader cameraSceneLoader,
            DetailSceneLoader detailSceneLoader,
            CameraSceneSettings settings,
            CameraZoomerData zoomerData,
            IPlayerStateProvider playerStateProvider,
            IRoadNodeLookup nodeLookup)
        {
            _cameraController = cameraController;
            _cameraSceneLoader = cameraSceneLoader;
            _detailSceneLoader = detailSceneLoader;
            _settings = settings;
            _zoomerData = zoomerData;
            _playerStateProvider = playerStateProvider;
            _nodeLookup = nodeLookup;

            _cameraSceneLoader.OnDetailSceneAutoUnloaded += HandleDetailSceneAutoUnloaded;
        }

        public bool CanEnterCity(CityData city)
        {
            if (city == null) return false;
            if (_isTransitioning) return false;
            if (IsInCityView) return false;
            if (_playerStateProvider.State != PlayerState.Idle) return false;

            return city.DetailScene != null &&
                   !string.IsNullOrEmpty(city.DetailScene.SceneName);
        }

        public void EnterCity(CityData city, Action onComplete = null)
        {
            if (!CanEnterCity(city))
            {
                Debug.LogWarning($"[CityEntryService] Cannot enter city {city?.Id}: conditions not met");
                onComplete?.Invoke();
                return;
            }

            _isTransitioning = true;
            _currentCity = city;
            _cameraSceneLoader.SuspendAutoLoading = true;

            UnityEngine.Camera cam = UnityEngine.Camera.main;
            _cameraYRotation = cam.transform.eulerAngles.y;
            _previousCameraSize = _cameraController.CurrentZoomSize;
            _previousWorldTarget = GetWorldTarget(cam);

            Vector3? nodePos = _nodeLookup.GetPosition(city.NodeId);
            if (!nodePos.HasValue)
            {
                Debug.LogWarning($"[CityEntryService] Cannot find node position for {city.NodeId}");
                _isTransitioning = false;
                _currentCity = null;
                _cameraSceneLoader.SuspendAutoLoading = false;
                onComplete?.Invoke();
                return;
            }

            Vector2 targetPosition = new Vector2(nodePos.Value.x, nodePos.Value.z);
            SceneReference detailScene = city.DetailScene;
            float targetSize = detailScene.CameraSize > 0
                ? detailScene.CameraSize
                : _settings.DefaultCityDetailZoomSize;

            void OnEnterComplete()
            {
                _isTransitioning = false;
                _cameraSceneLoader.SuspendAutoLoading = false;
                OnCityEntered?.Invoke(city);
                onComplete?.Invoke();
            }

            if (_settings.PreloadCityScene)
            {
                LoadDetailScene(city, () =>
                    AnimateCityTransition(
                        _previousWorldTarget, targetPosition,
                        _previousCameraSize, targetSize,
                        _settings.StrategicTiltAngle, _settings.DetailTiltAngle,
                        0.6f, OnEnterComplete));
            }
            else
            {
                AnimateCityTransition(
                    _previousWorldTarget, targetPosition,
                    _previousCameraSize, targetSize,
                    _settings.StrategicTiltAngle, _settings.DetailTiltAngle,
                    0.6f, () => LoadDetailScene(city, OnEnterComplete));
            }
        }

        public void ExitCity(Action onComplete = null)
        {
            if (!IsInCityView)
            {
                Debug.LogWarning("[CityEntryService] Cannot exit city: not in city view");
                onComplete?.Invoke();
                return;
            }

            _isTransitioning = true;
            _cameraSceneLoader.SuspendAutoLoading = true;
            CityData exitingCity = _currentCity;

            Vector3? nodePosition = _nodeLookup.GetPosition(exitingCity.NodeId);
            if (!nodePosition.HasValue)
            {
                Debug.LogWarning($"[CityEntryService] Cannot find node position for {exitingCity.NodeId}");
                _isTransitioning = false;
                _cameraSceneLoader.SuspendAutoLoading = false;
                onComplete?.Invoke();
                return;
            }

            UnityEngine.Camera cam = UnityEngine.Camera.main;
            Vector2 currentWorldTarget = GetWorldTarget(cam);
            float currentSize = _cameraController.CurrentZoomSize;

            AnimateCityTransition(
                currentWorldTarget, _previousWorldTarget,
                currentSize, _previousCameraSize,
                _settings.DetailTiltAngle, _settings.StrategicTiltAngle,
                0.6f, () =>
                {
                    UnloadDetailScene(exitingCity);
                    _currentCity = null;
                    _isTransitioning = false;
                    _cameraSceneLoader.SuspendAutoLoading = false;

                    OnCityExited?.Invoke();
                    onComplete?.Invoke();
                });
        }

        private void AnimateCityTransition(
            Vector2 startWorldTarget, Vector2 endWorldTarget,
            float startSize, float endSize,
            float startAngle, float endAngle,
            float duration, Action onComplete)
        {
            _transitionTween?.Kill();

            UnityEngine.Camera cam = UnityEngine.Camera.main;
            float startY = cam.transform.position.y;
            float endY = SizeToY(endSize);
            float yRot = _cameraYRotation;

            _transitionTween = DOVirtual.Float(0f, 1f, duration, t =>
            {
                float angle = Mathf.Lerp(startAngle, endAngle, t);
                float y = Mathf.Lerp(startY, endY, t);
                float x = Mathf.Lerp(startWorldTarget.x, endWorldTarget.x, t);
                float wz = Mathf.Lerp(startWorldTarget.y, endWorldTarget.y, t);
                float zOffset = CalculateZOffset(y, angle, yRot);

                cam.transform.position = new Vector3(x, y, wz + zOffset);
                cam.transform.eulerAngles = new Vector3(angle, yRot, 0f);
            }).SetEase(Ease.InOutSine).OnComplete(() => onComplete?.Invoke());
        }

        private Vector2 GetWorldTarget(UnityEngine.Camera cam)
        {
            Vector3 forward = cam.transform.forward;
            Vector3 pos = cam.transform.position;
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

        private float SizeToY(float size)
        {
            return _zoomerData.BaseYPosition +
                   (size - _zoomerData.BaseSizeValue) / _zoomerData.ScaleFactor;
        }

        private void LoadDetailScene(CityData city, Action onComplete)
        {
            string sceneName = city.DetailScene.SceneName;
            Vector3? nodePos = _nodeLookup.GetPosition(city.NodeId);
            Vector2? origin = nodePos.HasValue
                ? new Vector2(nodePos.Value.x, nodePos.Value.z)
                : null;
            _cameraSceneLoader.SetActiveDetailScene(sceneName);
            _detailSceneLoader.LoadAndActivateScene(sceneName, origin, onComplete);
        }

        private void UnloadDetailScene(CityData city)
        {
            string sceneName = city.DetailScene.SceneName;
            _detailSceneLoader.DeactivateScene(sceneName);
            _cameraSceneLoader.SetActiveDetailScene(null);
        }

        private void HandleDetailSceneAutoUnloaded()
        {
            if (!IsInCityView) return;
            _currentCity = null;
            OnCityExited?.Invoke();
        }
    }
}

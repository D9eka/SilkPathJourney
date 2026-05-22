using System;
using Internal.Scripts.Camera;
using Internal.Scripts.Camera.Move;
using Internal.Scripts.Player;
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
        event Action<CityData> OnCityLeft;
        event Action<CityData> OnCityAutoApproached;
    }

    public class CityEntryService : ICityEntryService
    {
        private readonly CityViewAnimator _animator;
        private readonly CitySceneController _sceneController;
        private readonly CameraController _cameraController;
        private readonly CameraSceneLoader _cameraSceneLoader;
        private readonly CameraSceneSettings _settings;
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly CameraBounds _cameraBounds;
        private readonly ICameraMover _cameraMover;
        private readonly MainSceneVisibilityController _mainSceneVisibility;

        private CityData _currentCity;
        private bool _isTransitioning;
        private Vector2 _previousWorldTarget;
        private float _previousCameraSize;
        private float _cameraYRotation;

        public bool IsInCityView => _currentCity != null;
        public CityData CurrentCity => _currentCity;

        public event Action<CityData> OnCityEntered;
        public event Action OnCityExited;
        public event Action<CityData> OnCityLeft;
        public event Action<CityData> OnCityAutoApproached;

        public CityEntryService(
            CityViewAnimator animator,
            CitySceneController sceneController,
            CameraController cameraController,
            CameraSceneLoader cameraSceneLoader,
            CameraSceneSettings settings,
            IPlayerStateProvider playerStateProvider,
            CameraBounds cameraBounds,
            ICameraMover cameraMover,
            MainSceneVisibilityController mainSceneVisibility)
        {
            _animator = animator;
            _sceneController = sceneController;
            _cameraController = cameraController;
            _cameraSceneLoader = cameraSceneLoader;
            _settings = settings;
            _playerStateProvider = playerStateProvider;
            _cameraBounds = cameraBounds;
            _cameraMover = cameraMover;
            _mainSceneVisibility = mainSceneVisibility;

            _cameraSceneLoader.OnDetailSceneAutoUnloaded += HandleDetailSceneAutoUnloaded;
            _cameraSceneLoader.OnCityApproached += HandleCityApproached;
            _cameraSceneLoader.OnDetailSceneRestored += HandleDetailSceneRestored;
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
            _cameraMover.SuspendLateTick = true;

            UnityEngine.Camera cam = UnityEngine.Camera.main;
            _cameraYRotation = cam.transform.eulerAngles.y;
            _previousCameraSize = _cameraController.CurrentZoomSize;
            _previousWorldTarget = _animator.GetWorldTarget(cam);

            Vector2? cityPos = _sceneController.GetCityPosition(city);
            if (!cityPos.HasValue)
            {
                Debug.LogWarning($"[CityEntryService] Cannot find node position for {city.NodeId}");
                _isTransitioning = false;
                _currentCity = null;
                _cameraSceneLoader.SuspendAutoLoading = false;
                _cameraMover.SuspendLateTick = false;
                onComplete?.Invoke();
                return;
            }

            Vector2 targetPosition = cityPos.Value;
            SceneReference detailScene = city.DetailScene;
            float targetSize = _animator.YToSize(_settings.DetailSceneLoadThreshold);

            float currentY = cam.transform.position.y;
            float yRot = _cameraYRotation;

            void OnEnterComplete()
            {
                _mainSceneVisibility.Hide();
                _sceneController.ShowDetailRenderers(detailScene.SceneName);

                DetailSceneBounds sceneBounds = _sceneController.FindBounds(detailScene.SceneName);
                if (sceneBounds != null)
                    _cameraBounds.SetOverrideBounds(sceneBounds.Center, sceneBounds.Size);
                else
                    Debug.LogWarning($"[CityEntryService] DetailSceneBounds not found in scene {detailScene.SceneName}");

                _cameraMover.SuspendLateTick = false;
                _isTransitioning = false;
                _cameraSceneLoader.SuspendAutoLoading = false;
                OnCityEntered?.Invoke(city);
                onComplete?.Invoke();
            }

            if (_settings.PreloadCityScene)
            {
                _sceneController.LoadScene(city, hideMainScene: false, onComplete: () =>
                    _animator.AnimateMove(_previousWorldTarget, targetPosition, currentY,
                        _settings.StrategicTiltAngle, yRot, 0.4f, () =>
                            _animator.AnimateZoomTilt(targetPosition, _previousCameraSize, targetSize,
                                _settings.StrategicTiltAngle, _settings.DetailTiltAngle, yRot,
                                0.4f, OnEnterComplete)));
            }
            else
            {
                _animator.AnimateMove(_previousWorldTarget, targetPosition, currentY,
                    _settings.StrategicTiltAngle, yRot, 0.4f, () =>
                        _sceneController.LoadScene(city, hideMainScene: false, onComplete: () =>
                            _animator.AnimateZoomTilt(targetPosition, _previousCameraSize, targetSize,
                                _settings.StrategicTiltAngle, _settings.DetailTiltAngle, yRot,
                                0.4f, OnEnterComplete)));
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
            _cameraMover.SuspendLateTick = true;
            CityData exitingCity = _currentCity;

            Vector2? cityPos = _sceneController.GetCityPosition(exitingCity);
            if (!cityPos.HasValue)
            {
                Debug.LogWarning($"[CityEntryService] Cannot find node position for {exitingCity.NodeId}");
                _isTransitioning = false;
                _cameraSceneLoader.SuspendAutoLoading = false;
                _cameraMover.SuspendLateTick = false;
                onComplete?.Invoke();
                return;
            }

            UnityEngine.Camera cam = UnityEngine.Camera.main;
            Vector2 currentWorldTarget = _animator.GetWorldTarget(cam);
            float currentSize = _cameraController.CurrentZoomSize;
            Vector2 cityNodeTarget = cityPos.Value;

            float strategicY = _animator.SizeToY(_previousCameraSize);
            float yRot = _cameraYRotation;

            _mainSceneVisibility.Show();
            _cameraBounds.ClearOverrideBounds();

            _animator.AnimateZoomTilt(cityNodeTarget, currentSize, _previousCameraSize,
                _settings.DetailTiltAngle, _settings.StrategicTiltAngle, yRot, 0.4f, () =>
                    _animator.AnimateMove(cityNodeTarget, _previousWorldTarget, strategicY,
                        _settings.StrategicTiltAngle, yRot, 0.4f, () =>
                        {
                            _sceneController.UnloadScene(exitingCity, showMainScene: false);
                            _currentCity = null;
                            _isTransitioning = false;
                            _cameraSceneLoader.SuspendAutoLoading = false;
                            _cameraMover.SuspendLateTick = false;

                            OnCityLeft?.Invoke(exitingCity);
                            OnCityExited?.Invoke();
                            onComplete?.Invoke();
                        }));
        }

        private void HandleCityApproached(CityData city)
        {
            if (IsInCityView || _isTransitioning) return;

            CaptureStrategicCameraState();
            OnCityAutoApproached?.Invoke(city);
        }

        private void HandleDetailSceneRestored(CityData city)
        {
            if (_isTransitioning) return;

            CaptureStrategicCameraState();
            _currentCity = city;
            OnCityEntered?.Invoke(city);
        }

        private void CaptureStrategicCameraState()
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            _cameraYRotation = cam.transform.eulerAngles.y;
            _previousCameraSize = _cameraController.CurrentZoomSize;
            _previousWorldTarget = _animator.GetWorldTarget(cam);
        }

        private void HandleDetailSceneAutoUnloaded()
        {
            if (!IsInCityView) return;
            _currentCity = null;
            _cameraBounds.ClearOverrideBounds();
            OnCityExited?.Invoke();
        }
    }
}

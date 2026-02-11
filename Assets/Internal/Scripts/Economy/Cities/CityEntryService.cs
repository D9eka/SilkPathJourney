using System;
using Internal.Scripts.Camera;
using Internal.Scripts.Player;
using Internal.Scripts.Player.StartMovement;
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
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly IRoadNodeLookup _nodeLookup;
        private readonly IPlayerStartMovement _playerStartMovement;

        private CityData _currentCity;
        private bool _isTransitioning;
        private Vector3 _previousCameraPosition;
        private float _previousCameraSize;

        public bool IsInCityView => _currentCity != null;
        public CityData CurrentCity => _currentCity;

        public event Action<CityData> OnCityEntered;
        public event Action OnCityExited;

        public CityEntryService(
            CameraController cameraController,
            CameraSceneLoader cameraSceneLoader,
            DetailSceneLoader detailSceneLoader,
            CameraSceneSettings settings,
            IPlayerStateProvider playerStateProvider,
            IRoadNodeLookup nodeLookup,
            IPlayerStartMovement playerStartMovement)
        {
            _cameraController = cameraController;
            _cameraSceneLoader = cameraSceneLoader;
            _detailSceneLoader = detailSceneLoader;
            _settings = settings;
            _playerStateProvider = playerStateProvider;
            _nodeLookup = nodeLookup;
            _playerStartMovement = playerStartMovement;

            _cameraSceneLoader.OnDetailSceneAutoUnloaded += HandleDetailSceneAutoUnloaded;
        }

        public bool CanEnterCity(CityData city)
        {
            if (city == null) return false;
            if (_isTransitioning) return false;
            if (IsInCityView) return false;
            if (_playerStateProvider.State != PlayerState.Idle) return false;

            // City must have detail scene configured
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

            // Save current camera state for exit
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            _previousCameraPosition = cam.transform.position;
            _previousCameraSize = _cameraController.CurrentZoomSize;

            // Get target position from node world coordinates
            Vector3? nodePos = _nodeLookup.GetPosition(city.NodeId);
            if (!nodePos.HasValue)
            {
                Debug.LogWarning($"[CityEntryService] Cannot find node position for {city.NodeId}");
                _isTransitioning = false;
                _currentCity = null;
                onComplete?.Invoke();
                return;
            }

            Vector2 targetPosition = new Vector2(nodePos.Value.x, nodePos.Value.z);
            SceneReference detailScene = city.DetailScene;
            float targetSize = detailScene.CameraSize > 0
                ? detailScene.CameraSize
                : _settings.DefaultCityDetailZoomSize;

            // Load scene if preload enabled
            if (_settings.PreloadCityScene)
            {
                LoadDetailScene(city, () =>
                    AnimateCameraTransition(targetPosition, targetSize, 0.3f, () =>
                    {
                        _isTransitioning = false;
                        OnCityEntered?.Invoke(city);
                        onComplete?.Invoke();
                    }));
            }
            else
            {
                AnimateCameraTransition(targetPosition, targetSize, 0.3f, () =>
                {
                    LoadDetailScene(city, () =>
                    {
                        _isTransitioning = false;
                        OnCityEntered?.Invoke(city);
                        onComplete?.Invoke();
                    });
                });
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
            CityData exitingCity = _currentCity;

            // Get node position to center camera on
            Vector3? nodePosition = _nodeLookup.GetPosition(exitingCity.NodeId);
            if (!nodePosition.HasValue)
            {
                Debug.LogWarning($"[CityEntryService] Cannot find node position for {exitingCity.NodeId}");
                _isTransitioning = false;
                onComplete?.Invoke();
                return;
            }

            Vector2 nodeXZ = new Vector2(nodePosition.Value.x, nodePosition.Value.z);

            // Animate to strategic zoom + center on node
            AnimateCameraTransition(nodeXZ, _previousCameraSize, 0.3f, () =>
            {
                UnloadDetailScene(exitingCity);
                _currentCity = null;
                _isTransitioning = false;

                // Invoke exit event (PlayerController will update State → Idle)
                OnCityExited?.Invoke();

                // Start movement selection screen
                _playerStartMovement.BeginSelection();

                onComplete?.Invoke();
            });
        }

        private void AnimateCameraTransition(Vector2 position, float size, float duration, Action onComplete)
        {
            _cameraController.MoveCamera(position, duration, () =>
            {
                _cameraController.ZoomCamera(size, duration, () => onComplete?.Invoke());
            });
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

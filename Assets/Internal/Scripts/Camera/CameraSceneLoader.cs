using System;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Player;
using Internal.Scripts.Road.Nodes;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Internal.Scripts.Camera
{
    public class CameraSceneLoader : ITickable, IInitializable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly CameraSceneSettings _settings;
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly IRoadNodeLookup _nodeLookup;
        private readonly DetailSceneLoader _detailSceneLoader;

        private string _activeDetailScene;

        public bool SuspendAutoLoading { get; set; }
        public event Action OnDetailSceneAutoUnloaded;

        public CameraSceneLoader(
            UnityEngine.Camera camera,
            CameraSceneSettings settings,
            IPlayerStateProvider playerStateProvider,
            ICityNodeResolver cityNodeResolver,
            IRoadNodeLookup nodeLookup,
            DetailSceneLoader detailSceneLoader)
        {
            _camera = camera;
            _settings = settings;
            _playerStateProvider = playerStateProvider;
            _cityNodeResolver = cityNodeResolver;
            _nodeLookup = nodeLookup;
            _detailSceneLoader = detailSceneLoader;
        }

        public void Initialize()
        {
            _activeDetailScene = null;

            int loadedSceneCount = SceneManager.sceneCount;
            if (loadedSceneCount > 1)
            {
                for (int i = 0; i < loadedSceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.isLoaded && scene.name != _settings.MainScene.SceneName)
                    {
                        _activeDetailScene = scene.name;
                        break;
                    }
                }
            }
        }

        public void Tick()
        {
            if (SuspendAutoLoading)
                return;

            if (!_settings.EnableDetailSceneLoading || _detailSceneLoader.IsPendingLoad)
                return;

            PlayerState playerState = _playerStateProvider.State;
            if (playerState == PlayerState.Moving || playerState == PlayerState.SelectingTarget)
                return;

            float cameraY = _camera.transform.position.y;
            var sceneInfo = GetSceneToLoad();

            if (cameraY < _settings.DetailSceneLoadThreshold)
            {
                if (sceneInfo.HasValue && _activeDetailScene != sceneInfo.Value.sceneName)
                {
                    if (!string.IsNullOrEmpty(_activeDetailScene))
                        _detailSceneLoader.DeactivateScene(_activeDetailScene);

                    string sceneName = sceneInfo.Value.sceneName;
                    Vector2 origin = sceneInfo.Value.origin;
                    _detailSceneLoader.LoadAndActivateScene(sceneName, origin, () =>
                    {
                        _activeDetailScene = sceneName;
                    });
                }
            }
            else if (cameraY > _settings.DetailSceneUnloadThreshold && !string.IsNullOrEmpty(_activeDetailScene))
            {
                _detailSceneLoader.DeactivateScene(_activeDetailScene);
                _activeDetailScene = null;
                OnDetailSceneAutoUnloaded?.Invoke();
            }
        }

        private (string sceneName, Vector2 origin)? GetSceneToLoad()
        {
            string currentNodeId = _playerStateProvider.CurrentNodeId;
            if (string.IsNullOrEmpty(currentNodeId))
                return null;

            if (!_cityNodeResolver.TryGetCityByNodeId(currentNodeId, out CityData city))
                return null;

            if (city.DetailScene == null || string.IsNullOrEmpty(city.DetailScene.SceneName))
                return null;

            Vector3? nodePos = _nodeLookup.GetPosition(city.NodeId);
            if (!nodePos.HasValue)
                return null;

            Vector2 origin = new Vector2(nodePos.Value.x, nodePos.Value.z);
            return (city.DetailScene.SceneName, origin);
        }

        public void SetActiveDetailScene(string sceneName)
        {
            _activeDetailScene = sceneName;
        }
    }
}

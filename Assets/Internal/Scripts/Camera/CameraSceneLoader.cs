using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Internal.Scripts.Camera
{
    public class CameraSceneLoader : ITickable, IInitializable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly CameraSceneSettings _settings;

        private bool _detailSceneLoaded;
        private AsyncOperation _currentLoadOperation;
        private Dictionary<Renderer, bool> _mainSceneRenderersState = new Dictionary<Renderer, bool>();

        public CameraSceneLoader(UnityEngine.Camera camera, CameraSceneSettings settings)
        {
            _camera = camera;
            _settings = settings;
        }

        public void Initialize()
        {
            _detailSceneLoaded = false;

            // Проверить, загружена ли детальная сцена (случай загрузки из сейва)
            Scene detailScene = SceneManager.GetSceneByName(_settings.DetailScene.SceneName);
            _detailSceneLoaded = detailScene.isLoaded;

            // Если детальная сцена уже загружена, скрыть визуал главной
            if (_detailSceneLoaded)
            {
                HideMainSceneVisuals();
            }
        }

        public void Tick()
        {
            if (!_settings.EnableDetailSceneLoading || _currentLoadOperation != null)
                return;

            float cameraY = _camera.transform.position.y;

            if (!_detailSceneLoaded && cameraY < _settings.DetailSceneLoadThreshold)
            {
                LoadDetailScene();
            }
            else if (_detailSceneLoaded && cameraY > _settings.DetailSceneUnloadThreshold)
            {
                DeactivateDetailScene();
            }
        }

        private void LoadDetailScene()
        {
            if (string.IsNullOrEmpty(_settings.DetailScene.SceneName))
                return;

            Scene scene = SceneManager.GetSceneByName(_settings.DetailScene.SceneName);

            // If scene is already loaded but deactivated - reactivate it
            if (scene.isLoaded)
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    root.SetActive(true);
                }
                _detailSceneLoaded = true;
                Debug.Log($"[CameraSceneLoader] Reactivated detail scene: {_settings.DetailScene.SceneName}");
                HideMainSceneVisuals();
                return;
            }

            // Otherwise load as usual
            _currentLoadOperation = SceneManager.LoadSceneAsync(_settings.DetailScene.SceneName, LoadSceneMode.Additive);
            if (_currentLoadOperation != null)
            {
                _currentLoadOperation.completed += OnDetailSceneLoaded;
            }
        }

        private void DeactivateDetailScene()
        {
            if (string.IsNullOrEmpty(_settings.DetailScene.SceneName))
                return;

            Scene scene = SceneManager.GetSceneByName(_settings.DetailScene.SceneName);
            if (!scene.isLoaded)
            {
                _detailSceneLoaded = false;
                return;
            }

            // Deactivate all root GameObjects in the scene
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                root.SetActive(false);
            }

            _detailSceneLoaded = false;
            Debug.Log($"[CameraSceneLoader] Deactivated detail scene: {_settings.DetailScene.SceneName}");
            ShowMainSceneVisuals();
        }

        private void OnDetailSceneLoaded(AsyncOperation operation)
        {
            _detailSceneLoaded = true;
            _currentLoadOperation = null;
            Debug.Log($"[CameraSceneLoader] Loaded detail scene: {_settings.DetailScene.SceneName}");
            HideMainSceneVisuals();
        }

        private void HideMainSceneVisuals()
        {
            Scene mainScene = SceneManager.GetSceneByName(_settings.MainScene.SceneName);
            if (!mainScene.isLoaded)
                return;

            _mainSceneRenderersState.Clear();

            foreach (GameObject root in mainScene.GetRootGameObjects())
            {
                // Skip the camera object
                if (root.GetComponent<UnityEngine.Camera>() != null)
                    continue;

                // Cache and disable all renderers
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    _mainSceneRenderersState[renderer] = renderer.enabled;
                    renderer.enabled = false;
                }
            }

            Debug.Log($"[CameraSceneLoader] Hidden main scene visuals: {_settings.MainScene.SceneName}");
        }

        private void ShowMainSceneVisuals()
        {
            // Restore renderer states
            foreach (var kvp in _mainSceneRenderersState)
            {
                if (kvp.Key != null) // Check if renderer still exists
                    kvp.Key.enabled = kvp.Value; // Restore original state
            }

            Debug.Log($"[CameraSceneLoader] Shown main scene visuals: {_settings.MainScene.SceneName}");
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Internal.Scripts.Camera
{
    public class DetailSceneLoader
    {
        private readonly MainSceneVisibilityController _mainSceneVisibility;

        private string _pendingLoadScene;

        public bool IsPendingLoad => _pendingLoadScene != null;

        public DetailSceneLoader(MainSceneVisibilityController mainSceneVisibility)
        {
            _mainSceneVisibility = mainSceneVisibility;
        }

        public void LoadAndActivateScene(string sceneName, Vector2? sceneOrigin = null,
            Action onComplete = null, bool hideMainScene = true, bool hideRenderers = false)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                onComplete?.Invoke();
                return;
            }

            Scene scene = SceneManager.GetSceneByName(sceneName);

            if (scene.isLoaded)
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    root.SetActive(true);
                }
                Debug.Log($"[DetailSceneLoader] Reactivated scene: {sceneName}");
                if (hideRenderers) SetRenderersEnabled(sceneName, false);
                if (hideMainScene) _mainSceneVisibility.Hide();
                onComplete?.Invoke();
                return;
            }

            if (_pendingLoadScene == sceneName)
            {
                onComplete?.Invoke();
                return;
            }

            _pendingLoadScene = sceneName;
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (loadOp != null)
            {
                loadOp.completed += _ =>
                {
                    _pendingLoadScene = null;
                    if (sceneOrigin.HasValue)
                        PositionScene(sceneName, sceneOrigin.Value);
                    Debug.Log($"[DetailSceneLoader] Loaded scene: {sceneName}");
                    if (hideRenderers) SetRenderersEnabled(sceneName, false);
                    if (hideMainScene) _mainSceneVisibility.Hide();
                    onComplete?.Invoke();
                };
            }
        }

        public void DeactivateScene(string sceneName, bool showMainScene = true)
        {
            if (string.IsNullOrEmpty(sceneName))
                return;

            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded)
                return;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                root.SetActive(false);
            }

            Debug.Log($"[DetailSceneLoader] Deactivated scene: {sceneName}");
            if (showMainScene) _mainSceneVisibility.Show();
        }

        public void SetRenderersEnabled(string sceneName, bool enabled)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded) return;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
                    r.enabled = enabled;
            }
        }

        private void PositionScene(string sceneName, Vector2 worldXZ)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded) return;

            Vector3 offset = new Vector3(worldXZ.x, 0f, worldXZ.y);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                root.transform.position += offset;
            }
        }
    }
}

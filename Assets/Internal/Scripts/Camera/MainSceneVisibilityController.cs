using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Internal.Scripts.Camera
{
    public class MainSceneVisibilityController
    {
        private readonly CameraSceneSettings _settings;
        private readonly Dictionary<Renderer, bool> _renderersState = new Dictionary<Renderer, bool>();
        private bool _hidden;

        public MainSceneVisibilityController(CameraSceneSettings settings)
        {
            _settings = settings;
        }

        public void Hide()
        {
            if (_hidden) return;

            Scene mainScene = SceneManager.GetSceneByName(_settings.MainScene.SceneName);
            if (!mainScene.isLoaded)
                return;

            _hidden = true;
            _renderersState.Clear();

            foreach (GameObject root in mainScene.GetRootGameObjects())
            {
                if (root.GetComponent<UnityEngine.Camera>() != null)
                    continue;

                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    _renderersState[renderer] = renderer.enabled;
                    renderer.enabled = false;
                }
            }

            Debug.Log($"[MainSceneVisibility] Hidden main scene visuals: {_settings.MainScene.SceneName}");
        }

        public void Show()
        {
            if (!_hidden) return;
            _hidden = false;

            foreach (var kvp in _renderersState)
            {
                if (kvp.Key != null)
                    kvp.Key.enabled = kvp.Value;
            }

            Debug.Log($"[MainSceneVisibility] Shown main scene visuals: {_settings.MainScene.SceneName}");
        }
    }
}

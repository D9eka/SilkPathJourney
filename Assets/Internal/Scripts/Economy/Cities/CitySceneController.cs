using System;
using Internal.Scripts.Camera;
using Internal.Scripts.Road.Nodes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Internal.Scripts.Economy.Cities
{
    public class CitySceneController
    {
        private readonly CameraSceneLoader _cameraSceneLoader;
        private readonly DetailSceneLoader _detailSceneLoader;
        private readonly IRoadNodeLookup _nodeLookup;

        public CitySceneController(
            CameraSceneLoader cameraSceneLoader,
            DetailSceneLoader detailSceneLoader,
            IRoadNodeLookup nodeLookup)
        {
            _cameraSceneLoader = cameraSceneLoader;
            _detailSceneLoader = detailSceneLoader;
            _nodeLookup = nodeLookup;
        }

        public Vector2? GetCityPosition(CityData city)
        {
            Vector3? nodePos = _nodeLookup.GetPosition(city.NodeId);
            if (!nodePos.HasValue) return null;
            return new Vector2(nodePos.Value.x, nodePos.Value.z);
        }

        public void LoadScene(CityData city, bool hideMainScene, Action onComplete)
        {
            string sceneName = city.DetailScene.SceneName;
            Vector3? nodePos = _nodeLookup.GetPosition(city.NodeId);
            Vector2? origin = nodePos.HasValue
                ? new Vector2(nodePos.Value.x, nodePos.Value.z)
                : null;
            _cameraSceneLoader.SetActiveDetailScene(sceneName);
            _detailSceneLoader.LoadAndActivateScene(sceneName, origin, onComplete, hideMainScene);
        }

        public void UnloadScene(CityData city, bool showMainScene)
        {
            string sceneName = city.DetailScene.SceneName;
            _detailSceneLoader.DeactivateScene(sceneName, showMainScene);
            _cameraSceneLoader.SetActiveDetailScene(null);
        }

        public DetailSceneBounds FindBounds(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded) return null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                var bounds = root.GetComponentInChildren<DetailSceneBounds>();
                if (bounds != null) return bounds;
            }
            return null;
        }
    }
}

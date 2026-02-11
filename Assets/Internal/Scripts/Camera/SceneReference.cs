using UnityEngine;

namespace Internal.Scripts.Camera
{
    [System.Serializable]
    public class SceneReference : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [SerializeField] private UnityEditor.SceneAsset _sceneAsset;
#endif

        [SerializeField] private string _sceneName;

        public string SceneName => _sceneName;

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (_sceneAsset != null)
            {
                _sceneName = _sceneAsset.name;
            }
#endif
        }

        public void OnAfterDeserialize()
        {
        }
    }
}

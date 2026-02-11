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

        [Tooltip("Camera center position for this scene (XZ coordinates)")]
        [SerializeField] private Vector2 _cameraCenter;

        [Tooltip("Camera orthographic size for this scene")]
        [SerializeField] private float _cameraSize = 15f;

        public string SceneName => _sceneName;
        public Vector2 CameraCenter => _cameraCenter;
        public float CameraSize => _cameraSize;

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

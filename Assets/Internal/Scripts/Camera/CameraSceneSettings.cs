using UnityEngine;

namespace Internal.Scripts.Camera
{
    [CreateAssetMenu(menuName = "SPJ/Camera/Scene Settings", fileName = "CameraSceneSettings")]
    public class CameraSceneSettings : ScriptableObject
    {
        [Tooltip("Camera Y position below which detail scene should load")]
        [field: SerializeField] public float DetailSceneLoadThreshold { get; private set; } = 50f;

        [Tooltip("Camera Y position above which detail scene should unload")]
        [field: SerializeField] public float DetailSceneUnloadThreshold { get; private set; } = 60f;

        [Tooltip("Main strategic scene (always the same)")]
        [field: SerializeField] public SceneReference MainScene { get; private set; }

        [Tooltip("Detail scene to load when zoomed in")]
        [field: SerializeField] public SceneReference DetailScene { get; private set; }

        [field: SerializeField] public bool EnableDetailSceneLoading { get; private set; } = true;
    }
}

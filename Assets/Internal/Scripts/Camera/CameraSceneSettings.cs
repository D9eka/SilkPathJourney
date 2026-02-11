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

        [field: SerializeField] public bool EnableDetailSceneLoading { get; private set; } = true;

        [Header("City Detail Scene Settings")]
        [Tooltip("Default camera size for city detail scenes if not specified in SceneReference")]
        [field: SerializeField] public float DefaultCityDetailZoomSize { get; private set; } = 15f;

        [Tooltip("Load detail scene before animation starts (smoother) or during animation")]
        [field: SerializeField] public bool PreloadCityScene { get; private set; } = true;

        [Header("Camera Movement")]
        [Tooltip("Base movement speed multiplier")]
        [field: SerializeField] public float MoveSensitivity { get; private set; } = 1f;

        [Header("Camera Tilt")]
        [Tooltip("Camera X rotation for strategic (top-down) view")]
        [field: SerializeField] public float StrategicTiltAngle { get; private set; } = 90f;

        [Tooltip("Camera X rotation for detailed (isometric) view")]
        [field: SerializeField] public float DetailTiltAngle { get; private set; } = 45f;
    }
}

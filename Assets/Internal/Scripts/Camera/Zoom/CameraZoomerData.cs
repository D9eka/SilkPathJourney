using UnityEngine;

namespace Internal.Scripts.Camera.Zoom
{
    [CreateAssetMenu(menuName = "SPJ/Camera Zoomer Data", fileName = "Camera Zoomer Data")]
    public class CameraZoomerData : ScriptableObject
    {
        [field: SerializeField] public float MinValue { get; private set; }
        [field: SerializeField] public float MaxValue { get; private set; }
        [field: SerializeField] public float Sensitivity { get; private set; }
    }
}
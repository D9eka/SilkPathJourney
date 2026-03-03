using UnityEngine;

namespace Internal.Scripts.Camera.Zoom
{
    [CreateAssetMenu(menuName = "SPJ/Camera Zoomer Data", fileName = "Camera Zoomer Data")]
    public class CameraZoomerData : ScriptableObject
    {
        [field: SerializeField] public float MinValue { get; private set; }
        [field: SerializeField] public float MaxValue { get; private set; }
        [field: SerializeField] public float Sensitivity { get; private set; }

        [field: SerializeField] public float BaseYPosition { get; private set; } = -10f;
        [field: SerializeField] public float BaseSizeValue { get; private set; } = 5f;
        [field: SerializeField] public float ScaleFactor { get; private set; } = 0.5f;

        [field: SerializeField] public bool EnableZoomToCursor { get; private set; } = true;
        [field: SerializeField] public float ZoomSpeed { get; private set; } = 10f;

        public float SizeToY(float size) => BaseYPosition + (size - BaseSizeValue) / ScaleFactor;
        public float YToSize(float y) => BaseSizeValue + (y - BaseYPosition) * ScaleFactor;
    }
}
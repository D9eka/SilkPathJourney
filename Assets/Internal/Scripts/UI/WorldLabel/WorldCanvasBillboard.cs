using Internal.Scripts.Camera.Zoom;
using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel
{
    public class WorldCanvasBillboard : MonoBehaviour
    {
        private UnityEngine.Camera _camera;
        private float _minScale;
        private float _maxScale;
        private float _minCameraY;
        private float _maxCameraY;

        public void Initialize(UnityEngine.Camera camera, WorldCanvasSettings settings, CameraZoomerData zoomerData)
        {
            _camera = camera;
            _minScale = settings.MinLabelScale;
            _maxScale = settings.MaxLabelScale;
            _minCameraY = zoomerData.BaseYPosition +
                (zoomerData.MinValue - zoomerData.BaseSizeValue) / zoomerData.ScaleFactor;
            _maxCameraY = zoomerData.BaseYPosition +
                (zoomerData.MaxValue - zoomerData.BaseSizeValue) / zoomerData.ScaleFactor;
        }

        private void LateUpdate()
        {
            if (_camera == null) return;

            transform.rotation = _camera.transform.rotation;

            float t = Mathf.InverseLerp(_maxCameraY, _minCameraY, _camera.transform.position.y);
            float scale = Mathf.Lerp(_minScale, _maxScale, t);
            transform.localScale = Vector3.one * scale;
        }
    }
}

using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel
{
    public class WorldCanvasBillboard : MonoBehaviour
    {
        private UnityEngine.Camera _camera;
        private float _baseScale;
        private AnimationCurve _curve;

        public void Initialize(UnityEngine.Camera camera, WorldCanvasSettings settings)
        {
            _camera = camera;
            _baseScale = settings.BaseScale;
            _curve = settings.DistanceScaleCurve;
        }

        private void LateUpdate()
        {
            if (_camera == null) return;

            transform.rotation = _camera.transform.rotation;
            float distance = Vector3.Distance(_camera.transform.position, transform.position);
            float scale = _baseScale * _curve.Evaluate(distance);
            transform.localScale = Vector3.one * scale;
        }
    }
}

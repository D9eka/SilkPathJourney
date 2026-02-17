using UnityEngine;

namespace Internal.Scripts.UI.PathVisualization
{
    public class PathLineView : MonoBehaviour
    {
        private LineRenderer _lineRenderer;
        private Material _material;

        public void Initialize(Material material)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _material = material;

            _lineRenderer.material = _material;
            _lineRenderer.startWidth = 0.4f;
            _lineRenderer.endWidth = 0.4f;
            _lineRenderer.startColor = Color.yellow;
            _lineRenderer.endColor = Color.yellow;
            _lineRenderer.numCornerVertices = 5;
            _lineRenderer.numCapVertices = 5;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            gameObject.SetActive(false);
        }

        public void SetPositions(Vector3[] positions)
        {
            if (_lineRenderer == null) return;
            _lineRenderer.positionCount = positions.Length;
            _lineRenderer.SetPositions(positions);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

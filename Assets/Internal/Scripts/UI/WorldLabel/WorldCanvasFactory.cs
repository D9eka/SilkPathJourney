using Internal.Scripts.Camera;
using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.UI.Arrow.PositionCalculation;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.WorldLabel
{
    public sealed class WorldCanvasFactory
    {
        private readonly DetailSceneBounds _bounds;
        private readonly TooltipService _tooltipService;
        private readonly GroundSnapper _groundSnapper;
        private readonly UnityEngine.Camera _camera;
        private readonly WorldCanvasSettings _settings;
        private readonly CameraZoomerData _zoomerData;

        public WorldCanvasFactory(
            DetailSceneBounds bounds,
            TooltipService tooltipService,
            GroundSnapper groundSnapper,
            UnityEngine.Camera camera,
            WorldCanvasSettings settings,
            CameraZoomerData zoomerData)
        {
            _bounds = bounds;
            _tooltipService = tooltipService;
            _groundSnapper = groundSnapper;
            _camera = camera;
            _settings = settings;
            _zoomerData = zoomerData;
        }

        public WorldCanvas Create()
        {
            var go = new GameObject("WorldCanvas");

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = _camera;
            canvas.sortingOrder = 100;
            go.AddComponent<GraphicRaycaster>();

            var rt = go.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;

            if (_bounds != null)
            {
                Bounds b = _bounds.BoundsCollider.bounds;
                go.transform.position = new Vector3(b.center.x, 0f, b.center.z);
            }

            var worldCanvas = go.AddComponent<WorldCanvas>();
            worldCanvas.Initialize(canvas, _tooltipService, _groundSnapper, _camera, _settings, _zoomerData);
            return worldCanvas;
        }
    }
}

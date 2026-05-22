using Internal.Scripts.Camera;
using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.UI.Arrow.PositionCalculation;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Internal.Scripts.UI.WorldLabel
{
    public sealed class WorldCanvasFactory
    {
        private readonly DetailSceneBounds _bounds;
        private readonly TooltipService _tooltipService;
        private readonly LocalizationService _localizationService;
        private readonly GroundSnapper _groundSnapper;
        private readonly UnityEngine.Camera _camera;
        private readonly WorldCanvasSettings _settings;
        private readonly CameraZoomerData _zoomerData;
        private readonly ScreenStackService _screenStackService;

        public WorldCanvasFactory(
            DetailSceneBounds bounds,
            TooltipService tooltipService,
            LocalizationService localizationService,
            GroundSnapper groundSnapper,
            UnityEngine.Camera camera,
            WorldCanvasSettings settings,
            CameraZoomerData zoomerData,
            ScreenStackService screenStackService)
        {
            _bounds = bounds;
            _tooltipService = tooltipService;
            _localizationService = localizationService;
            _groundSnapper = groundSnapper;
            _camera = camera;
            _settings = settings;
            _zoomerData = zoomerData;
            _screenStackService = screenStackService;
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
                SceneManager.MoveGameObjectToScene(go, _bounds.gameObject.scene);
            }

            var worldCanvas = go.AddComponent<WorldCanvas>();
            worldCanvas.Initialize(canvas, _tooltipService, _localizationService, _groundSnapper, _camera, _settings, _zoomerData, _screenStackService);
            return worldCanvas;
        }
    }
}

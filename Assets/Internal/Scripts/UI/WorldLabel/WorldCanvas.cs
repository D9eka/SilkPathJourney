using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.UI.Arrow.PositionCalculation;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel
{
    public class WorldCanvas : MonoBehaviour
    {
        private Canvas _canvas;
        private TooltipService _tooltipService;
        private LocalizationService _localizationService;
        private GroundSnapper _groundSnapper;
        private UnityEngine.Camera _camera;
        private WorldCanvasSettings _settings;
        private CameraZoomerData _zoomerData;
        private bool _isInitialized;

        public void Initialize(
            Canvas canvas,
            TooltipService tooltipService,
            LocalizationService localizationService,
            GroundSnapper groundSnapper,
            UnityEngine.Camera camera,
            WorldCanvasSettings settings,
            CameraZoomerData zoomerData)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[WorldCanvas] Already initialized.");
                return;
            }

            _canvas = canvas;
            _tooltipService = tooltipService;
            _localizationService = localizationService;
            _groundSnapper = groundSnapper;
            _camera = camera;
            _settings = settings;
            _zoomerData = zoomerData;
            _isInitialized = true;
        }

        public WorldLabelView CreateLabel(Vector3 worldPosition, string goName = "Label")
        {
            return CreateLabel(worldPosition, Vector3.zero, goName);
        }

        public WorldLabelView CreateLabel(Vector3 worldPosition, Vector3 offset, string goName = "Label")
        {
            if (!_isInitialized || _canvas == null)
            {
                Debug.LogError("[WorldCanvas] Not initialized before CreateLabel call.");
                return null;
            }

            WorldLabelView label = Instantiate(_settings.LabelPrefab, _canvas.transform);
            label.gameObject.name = goName;

            PositionElement(label.GetComponent<RectTransform>(), worldPosition, offset);
            AddBillboard(label.gameObject);
            label.Initialize(_tooltipService, _localizationService);

            return label;
        }

        public RoadLabelView CreateRoadLabel(Vector3 worldPosition, string goName = "RoadLabel")
        {
            if (!_isInitialized || _canvas == null)
            {
                Debug.LogError("[WorldCanvas] Not initialized before CreateRoadLabel call.");
                return null;
            }

            if (_settings.RoadLabelPrefab == null)
            {
                Debug.LogWarning("[WorldCanvas] RoadLabelPrefab is not assigned in WorldCanvasSettings.");
                return null;
            }

            RoadLabelView label = Instantiate(_settings.RoadLabelPrefab, _canvas.transform);
            label.gameObject.name = goName;

            PositionElement(label.GetComponent<RectTransform>(), worldPosition, Vector3.zero);
            AddBillboard(label.gameObject);
            label.Initialize(_tooltipService);

            return label;
        }

        public WorldCanvasBillboard AddBillboard(GameObject target)
        {
            var bb = target.AddComponent<WorldCanvasBillboard>();
            bb.Initialize(_camera, _settings, _zoomerData);
            return bb;
        }

        public RectTransform CreatePositionedRoot(Vector3 worldPosition, Vector3 offset, string goName)
        {
            if (!_isInitialized || _canvas == null) return null;

            var go = new GameObject(goName);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            go.transform.SetParent(_canvas.transform, false);

            PositionElement(rt, worldPosition, offset);

            return rt;
        }

        private void PositionElement(RectTransform rt, Vector3 worldPosition, Vector3 offset)
        {
            float offsetAboveGround = _settings != null ? _settings.OffsetAboveGround : 0.1f;
            Vector3 snappedPos = _groundSnapper != null
                ? _groundSnapper.SnapToGround(worldPosition, offsetAboveGround)
                : worldPosition + Vector3.up * offsetAboveGround;
            rt.anchoredPosition3D = _canvas.transform.InverseTransformPoint(snappedPos + offset);
        }
    }
}

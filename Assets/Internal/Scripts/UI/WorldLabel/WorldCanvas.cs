using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.UI.Arrow.PositionCalculation;
using Internal.Scripts.UI.Tooltip;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.WorldLabel
{
    public class WorldCanvas : MonoBehaviour
    {
        private Canvas _canvas;
        private TooltipService _tooltipService;
        private GroundSnapper _groundSnapper;
        private UnityEngine.Camera _camera;
        private WorldCanvasSettings _settings;
        private CameraZoomerData _zoomerData;
        private bool _isInitialized;

        public void Initialize(
            Canvas canvas,
            TooltipService tooltipService,
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

            GameObject labelGo = new GameObject(goName);
            RectTransform labelRt = labelGo.AddComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0.5f, 0.5f);
            labelRt.anchorMax = new Vector2(0.5f, 0.5f);
            labelGo.transform.SetParent(_canvas.transform, false);

            float offsetAboveGround = _settings != null ? _settings.OffsetAboveGround : 0.1f;
            Vector3 snappedPos = _groundSnapper != null
                ? _groundSnapper.SnapToGround(worldPosition, offsetAboveGround)
                : worldPosition + Vector3.up * offsetAboveGround;
            Vector3 localPos = _canvas.transform.InverseTransformPoint(snappedPos + offset);
            labelRt.anchoredPosition3D = localPos;

            float canvasScale = _settings != null ? _settings.CanvasScale : 0.02f;
            labelGo.transform.localScale = Vector3.one * canvasScale;

            GameObject bgGo = new GameObject("Background");
            bgGo.transform.SetParent(labelGo.transform, false);
            Image bg = bgGo.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0);
            bg.raycastTarget = true;
            RectTransform bgRt = bg.GetComponent<RectTransform>();
            bgRt.sizeDelta = new Vector2(100, 80);
            bgRt.anchoredPosition = Vector2.zero;

            GameObject textGo = new GameObject("NameText");
            textGo.transform.SetParent(labelGo.transform, false);
            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            int fontSize = _settings != null ? _settings.FontSize : 36;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;

            RectTransform textRt = tmp.GetComponent<RectTransform>();
            textRt.sizeDelta = new Vector2(300, 50);
            textRt.anchoredPosition = Vector2.zero;

            GameObject iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(labelGo.transform, false);
            Image icon = iconGo.AddComponent<Image>();
            icon.preserveAspect = true;

            RectTransform iconRt = icon.GetComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(40, 40);
            iconRt.anchoredPosition = new Vector2(0, 35);
            iconGo.SetActive(false);

            WorldLabelView label = labelGo.AddComponent<WorldLabelView>();
            label.Initialize(tmp, icon, _tooltipService, _camera, _settings, _zoomerData);

            return label;
        }
    }
}

using System.Collections.Generic;
using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.Road.Core;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Internal.Scripts.UI.WorldLabel
{
    public class RoadLabelView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const float HIGHLIGHT_Y_OFFSET = 0.15f;
        private const float HIGHLIGHT_WIDTH = 2f;

        [SerializeField] private Image _background;
        [SerializeField] private RectTransform _iconsContainer;
        [SerializeField] private Image _iconTemplate;

        [Header("Highlight")]
        [SerializeField] private Color _highlightColor = Color.yellow;

        private TooltipService _tooltipService;
        private UnityEngine.Camera _camera;
        private WorldCanvasSettings _settings;
        private float _minCameraY;
        private float _maxCameraY;

        private RoadRuntime _road;
        private LineRenderer _highlightLine;

        public void Initialize(TooltipService tooltipService, UnityEngine.Camera camera,
            WorldCanvasSettings settings, CameraZoomerData zoomerData)
        {
            _tooltipService = tooltipService;
            _camera = camera;
            _settings = settings;

            _minCameraY = zoomerData.BaseYPosition +
                (zoomerData.MinValue - zoomerData.BaseSizeValue) / zoomerData.ScaleFactor;
            _maxCameraY = zoomerData.BaseYPosition +
                (zoomerData.MaxValue - zoomerData.BaseSizeValue) / zoomerData.ScaleFactor;

            if (_background != null)
                _background.raycastTarget = true;

            if (_iconTemplate != null)
                _iconTemplate.gameObject.SetActive(false);
        }

        public void SetRoad(RoadRuntime road)
        {
            _road = road;
            if (road == null || road.Data == null) return;

            List<Vector3> points = road.Data.PointsLocal;
            if (points == null || points.Count < 2) return;

            var go = new GameObject("RoadHighlight");
            go.transform.SetParent(road.WorldRoot != null ? road.WorldRoot : road.transform, false);

            _highlightLine = go.AddComponent<LineRenderer>();
            _highlightLine.useWorldSpace = false;
            _highlightLine.positionCount = points.Count;

            for (int i = 0; i < points.Count; i++)
                _highlightLine.SetPosition(i, points[i] + Vector3.up * HIGHLIGHT_Y_OFFSET);

            _highlightLine.startWidth = HIGHLIGHT_WIDTH;
            _highlightLine.endWidth = HIGHLIGHT_WIDTH;

            var mat = new Material(Shader.Find("Sprites/Default"));
            Color c = _highlightColor;
            c.a = 0.5f;
            mat.color = c;
            _highlightLine.material = mat;
            _highlightLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _highlightLine.receiveShadows = false;

            go.SetActive(false);
        }

        public void AddIcon(Sprite icon, string name, string description)
        {
            if (_iconTemplate == null || _iconsContainer == null) return;

            GameObject iconGo = Instantiate(_iconTemplate.gameObject, _iconsContainer);
            iconGo.SetActive(true);

            Image image = iconGo.GetComponent<Image>();
            if (image != null)
                image.sprite = icon;

            WorldLabelIcon labelIcon = iconGo.AddComponent<WorldLabelIcon>();
            labelIcon.Initialize(_tooltipService, name, description);
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHighlight(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHighlight(false);
        }

        private void SetHighlight(bool enabled)
        {
            if (_highlightLine == null) return;
            _highlightLine.gameObject.SetActive(enabled);
        }

        private void LateUpdate()
        {
            if (_camera == null || _settings == null) return;

            transform.rotation = _camera.transform.rotation;

            float t = Mathf.InverseLerp(_maxCameraY, _minCameraY, _camera.transform.position.y);
            float scale = Mathf.Lerp(_settings.MinLabelScale, _settings.MaxLabelScale, t);
            transform.localScale = Vector3.one * scale;
        }

        private void OnDestroy()
        {
            if (_highlightLine != null)
                Destroy(_highlightLine.gameObject);
        }
    }
}

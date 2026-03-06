using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Road.Core;
using Internal.Scripts.UI.Theme;
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

        private StaticColorController _colorController;
        private Biome _biome = Biome.Plains;
        private TooltipService _tooltipService;

        private RoadRuntime _road;
        private LineRenderer _highlightLine;

        public void SetColorController(StaticColorController controller, Biome biome)
        {
            _colorController = controller;
            _biome = biome;
            gameObject.InitializeColorBinders(colorController: controller, biome: biome);
        }

        public void Initialize(TooltipService tooltipService)
        {
            _tooltipService = tooltipService;

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
            if (_colorController != null)
                c = _colorController.GetColor(_biome, ColorSlot.RoadHighlight);
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

        private void OnDestroy()
        {
            if (_highlightLine != null)
                Destroy(_highlightLine.gameObject);
        }
    }
}

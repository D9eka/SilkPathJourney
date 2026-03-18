using Internal.Scripts.UI.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Internal.Scripts.UI.WorldLabel.Components
{
    [RequireComponent(typeof(Image))]
    public class WorldLabelIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private string _tooltipTitle;
        private string _tooltipDescription;

        private Image _icon;
        private TooltipService _tooltipService;

        private void Awake()
        {
            _icon = GetComponent<Image>();
        }

        public void Initialize(TooltipService tooltipService)
        {
            _tooltipService = tooltipService;
        }

        public void Initialize(TooltipService tooltipService, string title, string description)
        {
            _tooltipService = tooltipService;
            _tooltipTitle = title;
            _tooltipDescription = description;
        }

        public void Initialize(TooltipService tooltipService, string title, string description, Sprite icon, float alpha = 1f)
        {
            _tooltipService = tooltipService;
            _tooltipTitle = title;
            _tooltipDescription = description;
            SetIcon(icon, alpha);
        }

        public void SetIcon(Sprite icon, float alpha = 1f)
        {
            if (_icon == null) return;
            _icon.sprite = icon;
            if (alpha < 1f)
            {
                var c = _icon.color;
                c.a = alpha;
                _icon.color = c;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tooltipService == null) return;
            var data = new SimpleTooltipData(_tooltipTitle, _tooltipDescription);
            _tooltipService.ShowTooltipDelayed(data, transform.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _tooltipService?.HideTooltip();
        }
    }
}

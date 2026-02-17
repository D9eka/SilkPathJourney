using Internal.Scripts.UI.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Internal.Scripts.UI.WorldLabel
{
    public class WorldLabelIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string _tooltipTitle;
        [SerializeField] private string _tooltipDescription;

        private TooltipService _tooltipService;

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

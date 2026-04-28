using System;
using Internal.Scripts.UI.Tooltip;
using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    public class IconLabel : MonoBehaviour
    {
        [SerializeField] private IconView _iconView;
        [SerializeField] private TextMeshProUGUI _label;

        private HoverReporter _hover;
        private Action _hoverEnter;
        private Action _hoverExit;

        public void Initialize(Sprite icon, string label)
        {
            _iconView.gameObject.SetActive(icon != null);
            if (icon != null)
                _iconView.SetSprite(icon);
            SetLabel(label);
        }

        public void SetLabel(string label)
        {
            if (_label != null)
                _label.text = label ?? string.Empty;
        }

        public void SetTooltip(ITooltipDataProvider provider, TooltipService service)
        {
            ClearTooltip();

            if (provider == null || service == null)
                return;

            _hover = HoverReporter.GetOrAdd(gameObject);
            _hoverEnter = () => service.ShowTooltipDelayed(provider);
            _hoverExit = () => service.HideTooltip();
            _hover.Entered += _hoverEnter;
            _hover.Exited += _hoverExit;
        }

        public void ClearTooltip()
        {
            if (_hover != null)
            {
                if (_hoverEnter != null) _hover.Entered -= _hoverEnter;
                if (_hoverExit != null) _hover.Exited -= _hoverExit;
            }
            _hoverEnter = null;
            _hoverExit = null;
        }

        private void OnDisable()
        {
            ClearTooltip();
        }
    }
}

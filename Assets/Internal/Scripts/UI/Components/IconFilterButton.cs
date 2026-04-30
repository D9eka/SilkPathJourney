using System;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class IconFilterButton : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private GameObject _border;

        private Button _button;
        private TooltipService _tooltipService;
        private HoverReporter _hover;
        private Action _clickHandler;
        private Action _hoverEnter;
        private Action _hoverExit;
        private string _tooltipTitle;

        public Button Button
        {
            get
            {
                if (_button == null)
                    _button = GetComponent<Button>();
                return _button;
            }
        }

        public void Configure(Sprite icon, string tooltipTitle, TooltipService tooltipService, Action onClick)
        {
            _tooltipService = tooltipService;
            _tooltipTitle = tooltipTitle;

            if (_iconImage != null && icon != null)
                _iconImage.sprite = icon;

            UnbindInternal();
            _clickHandler = onClick;
            Button.onClick.AddListener(Invoke);

            _hover = HoverReporter.GetOrAdd(gameObject);
            _hoverEnter = () => _tooltipService?.ShowTooltipDelayed(new SimpleTooltipData(_tooltipTitle, string.Empty));
            _hoverExit = () => _tooltipService?.HideTooltip();
            _hover.Entered += _hoverEnter;
            _hover.Exited += _hoverExit;
        }

        public void SetActive(bool active)
        {
            if (_border != null)
                _border.SetActive(active);
        }

        public void Unbind() => UnbindInternal();

        private void UnbindInternal()
        {
            if (_button != null && _clickHandler != null)
                _button.onClick.RemoveListener(Invoke);

            if (_hover != null)
            {
                if (_hoverEnter != null) _hover.Entered -= _hoverEnter;
                if (_hoverExit != null) _hover.Exited -= _hoverExit;
            }

            _clickHandler = null;
            _hoverEnter = null;
            _hoverExit = null;
        }

        private void Invoke() => _clickHandler?.Invoke();

        private void OnDestroy() => UnbindInternal();
    }
}

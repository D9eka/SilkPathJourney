using System;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Core.Tabs
{
    public class TabButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Image _background;

        private LocalizationService.LocalizedTextHandle _labelHandle;
        private Action<TabButton> _onSelected;
        private UnityAction _clickHandler;

        public void BindLabel(LocalizationService localization, LocalizedString localized, string context)
        {
            DisposeBinding();
            if (localization == null || localized == null) return;
            _labelHandle = localization.BindText(_label, localized, context);
        }

        public void AddListener(Action<TabButton> onSelected)
        {
            RemoveListener();
            _onSelected = onSelected;
            _clickHandler = HandleClick;
            _button.onClick.AddListener(_clickHandler);
        }

        public void RemoveListener()
        {
            if (_clickHandler != null)
                _button.onClick.RemoveListener(_clickHandler);

            _clickHandler = null;
            _onSelected = null;
        }

        public void SetSelected(bool selected)
        {
            _button.interactable = !selected;
            _button.gameObject.SetActive(!selected);
        }

        public void SetSelected(bool selected, (Color backgroundColor, Color textColor) colors)
        {
            _background.color = colors.backgroundColor;
            _label.color = colors.textColor;
            SetSelected(selected);
        }

        public void DisposeBinding()
        {
            _labelHandle?.Dispose();
            _labelHandle = null;
        }

        private void HandleClick()
        {
            _onSelected?.Invoke(this);
        }
    }
}

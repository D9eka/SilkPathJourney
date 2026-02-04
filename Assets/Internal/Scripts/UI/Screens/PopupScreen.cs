using System;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screen.View;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens
{
    public abstract class PopupScreen : ScreenViewBase
    {
        [Header("Headers")]
        [SerializeField] protected GameObject _mainHeader;
        [SerializeField] protected GameObject _additionalHeader;
        [Header("Texts")]
        [SerializeField] protected TextMeshProUGUI _mainHeaderText;
        [SerializeField] protected TextMeshProUGUI _additionalHeaderText;
        [Header("Buttons")]
        [SerializeField] protected Button _closeButton;
        [Header("Content")]
        [SerializeField] protected OverlayScreen _overlayScreen;
        [Header("LocalizedStrings")]
        [SerializeField] protected LocalizedString _mainHeaderLocalizedString;
        [SerializeField] protected LocalizedString _additionalHeaderLocalizedString;
        [Header("Colors")]
        [SerializeField] protected Color _additionalHeaderWarningColor = new Color(1f, 0.85f, 0.2f, 1f);
        
        private Color _additionalHeaderNormalColor;
        private LocalizationHelper.LocalizedTextHandle _mainHeaderHandle;
        private LocalizationHelper.LocalizedTextHandle _additionalHeaderHandle;
        private string _additionalHeaderValue = string.Empty;
        private object[] _additionalHeaderArgs;

        protected void Awake()
        {
            _additionalHeaderNormalColor = _additionalHeaderText.color;
        }

        protected virtual void OnEnable()
        {
            _closeButton.onClick.AddListener(HandleCloseClicked);
            BindLocalization();
        }

        protected virtual void OnDisable()
        {
            _closeButton.onClick.RemoveListener(HandleCloseClicked);
            _mainHeaderHandle?.Dispose();
            _mainHeaderHandle = null;
            _additionalHeaderHandle?.Dispose();
            _additionalHeaderHandle = null;
        }

        public void SetAdditionalHeaderState(bool state)
        {
            _additionalHeader.SetActive(state);
        }

        public void SetAdditionalHeaderText(string value, params object[] args)
        {
            _additionalHeaderValue = value ?? string.Empty;
            _additionalHeaderArgs = args != null && args.Length > 0
                ? args
                : new object[] { _additionalHeaderValue };
            if (_additionalHeaderHandle != null)
                _additionalHeaderHandle.SetArguments(_additionalHeaderValue, _additionalHeaderArgs);
            else
                _additionalHeaderText.text = _additionalHeaderValue;
        }

        public void SetAdditionalHeaderHighlight(bool state)
        {
            _additionalHeaderText.color = state ? _additionalHeaderWarningColor : _additionalHeaderNormalColor;
        }

        private void BindLocalization()
        {
            _mainHeaderHandle?.Dispose();
            _additionalHeaderHandle?.Dispose();
            _mainHeaderHandle = LocalizationHelper.BindText(_mainHeaderText, _mainHeaderLocalizedString, $"{name}.MainHeader");
            _additionalHeaderHandle = LocalizationHelper.BindText(_additionalHeaderText, _additionalHeaderLocalizedString, $"{name}.AdditionalHeader");
            if (!string.IsNullOrWhiteSpace(_additionalHeaderValue))
                _additionalHeaderHandle.SetArguments(
                    _additionalHeaderValue,
                    _additionalHeaderArgs ?? new object[] { _additionalHeaderValue });
        }

        private void HandleCloseClicked()
        {
            RaiseCloseRequested();
        }
    }
}

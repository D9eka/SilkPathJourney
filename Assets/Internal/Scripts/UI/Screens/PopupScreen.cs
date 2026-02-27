using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.View;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens
{
    public abstract class PopupScreen : ScreenViewBase
    {
        [Header("Components")]
        [SerializeField] protected HeaderElement _mainHeader;
        [SerializeField] protected CloseButton _closeButton;

        [Header("Content")]
        [SerializeField] protected OverlayScreen _overlayScreen;

        [Header("Header Localization")]
        [SerializeField] protected LocalizedString _mainHeaderLocalizedString;

        private LocalizationService.LocalizedTextHandle _mainHeaderHandle;

        protected override void OnLocalizationReady()
        {
            BindHeaderLocalization();
        }

        protected virtual void OnEnable()
        {
            if (_closeButton != null)
                _closeButton.Clicked += HandleCloseClicked;
            if (Localization != null)
                BindHeaderLocalization();
        }

        protected virtual void OnDisable()
        {
            if (_closeButton != null)
                _closeButton.Clicked -= HandleCloseClicked;
            _mainHeaderHandle?.Dispose();
            _mainHeaderHandle = null;
        }

        private void BindHeaderLocalization()
        {
            _mainHeaderHandle?.Dispose();
            if (_mainHeader != null && _mainHeader.Text != null && _mainHeaderLocalizedString != null)
                _mainHeaderHandle = Localization.BindText(_mainHeader.Text, _mainHeaderLocalizedString, $"{name}.MainHeader");
        }

        private void HandleCloseClicked() => RaiseCloseRequested();
    }
}

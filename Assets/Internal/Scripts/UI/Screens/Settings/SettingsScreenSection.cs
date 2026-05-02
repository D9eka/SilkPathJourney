using System;
using Internal.Scripts.UI.Localization;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Settings
{
    public class SettingsScreenSection : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private LocalizedString _headerLocalizedString;

        [Header("Fullscreen")]
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private TextMeshProUGUI _fullscreenText;
        [SerializeField] private LocalizedString _fullscreenLocalizedString;

        private SettingsScreenViewModel _viewModel;
        private IDisposable _subscription;
        private LocalizationService.LocalizedTextGroup _textGroup;

        public void Bind(SettingsScreenViewModel viewModel)
        {
            _viewModel = viewModel;
            _subscription?.Dispose();
            _subscription = _viewModel.Fullscreen.Subscribe(value => _fullscreenToggle.SetIsOnWithoutNotify(value));
        }

        public void BindLocalization(LocalizationService localization)
        {
            if (localization == null) return;
            _textGroup?.Dispose();
            _textGroup = localization.CreateTextGroup();
            _textGroup.Bind(_headerText, _headerLocalizedString, "Settings.ScreenHeader");
            _textGroup.Bind(_fullscreenText, _fullscreenLocalizedString, "Settings.Fullscreen");
        }

        private void OnEnable()
        {
            _fullscreenToggle.onValueChanged.AddListener(OnChanged);
        }

        private void OnDisable()
        {
            _fullscreenToggle.onValueChanged.RemoveListener(OnChanged);
            _subscription?.Dispose();
            _subscription = null;
            _textGroup?.Dispose();
            _textGroup = null;
        }

        private void OnChanged(bool isOn)
        {
            _viewModel?.SetFullscreen(isOn);
        }
    }
}

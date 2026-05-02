using System;
using Internal.Scripts.UI.Localization;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Settings
{
    public class SettingsLanguageSection : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private LocalizedString _headerLocalizedString;

        [Header("Toggles")]
        [SerializeField] private ToggleGroup _localeGroup;
        [SerializeField] private Toggle _localeRu;
        [SerializeField] private Toggle _localeEn;

        [Header("Toggle Labels")]
        [SerializeField] private TextMeshProUGUI _localeRuText;
        [SerializeField] private LocalizedString _localeRuLocalizedString;
        [SerializeField] private TextMeshProUGUI _localeEnText;
        [SerializeField] private LocalizedString _localeEnLocalizedString;

        private SettingsScreenViewModel _viewModel;
        private IDisposable _subscription;
        private LocalizationService.LocalizedTextGroup _textGroup;

        public void Bind(SettingsScreenViewModel viewModel)
        {
            _viewModel = viewModel;
            _subscription?.Dispose();
            _subscription = _viewModel.CurrentLocale.Subscribe(SyncToggles);
        }

        public void BindLocalization(LocalizationService localization)
        {
            if (localization == null) return;
            _textGroup?.Dispose();
            _textGroup = localization.CreateTextGroup();
            _textGroup.Bind(_headerText, _headerLocalizedString, "Settings.LanguageHeader");
            _textGroup.Bind(_localeRuText, _localeRuLocalizedString, "Settings.Locale.Ru");
            _textGroup.Bind(_localeEnText, _localeEnLocalizedString, "Settings.Locale.En");
        }

        private void OnEnable()
        {
            _localeRu.onValueChanged.AddListener(OnRuChanged);
            _localeEn.onValueChanged.AddListener(OnEnChanged);
        }

        private void OnDisable()
        {
            _localeRu.onValueChanged.RemoveListener(OnRuChanged);
            _localeEn.onValueChanged.RemoveListener(OnEnChanged);
            _subscription?.Dispose();
            _subscription = null;
            _textGroup?.Dispose();
            _textGroup = null;
        }

        private void SyncToggles(string code)
        {
            _localeRu.SetIsOnWithoutNotify(code == "ru");
            _localeEn.SetIsOnWithoutNotify(code == "en");
        }

        private void OnRuChanged(bool isOn)
        {
            if (isOn) _viewModel?.SelectLocale("ru");
        }

        private void OnEnChanged(bool isOn)
        {
            if (isOn) _viewModel?.SelectLocale("en");
        }
    }
}

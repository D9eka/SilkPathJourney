using Internal.Scripts.UI.Screens.Core.ViewModel;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Settings
{
    public class SettingsScreen : PopupScreen
    {
        [SerializeField] private SettingsLanguageSection _languageSection;
        [SerializeField] private SettingsScreenSection _screenSection;

        private SettingsScreenViewModel _viewModel;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as SettingsScreenViewModel;
            if (_viewModel == null) return;

            _languageSection?.Bind(_viewModel);
            _screenSection?.Bind(_viewModel);

            _languageSection?.BindLocalization(Localization);
            _screenSection?.BindLocalization(Localization);
        }

        protected override void OnLocalizationReady()
        {
            base.OnLocalizationReady();
            _languageSection?.BindLocalization(Localization);
            _screenSection?.BindLocalization(Localization);
        }
    }
}

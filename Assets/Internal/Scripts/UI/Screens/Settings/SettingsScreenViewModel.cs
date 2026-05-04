using Internal.Scripts.Settings;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using R3;

namespace Internal.Scripts.UI.Screens.Settings
{
    public sealed class SettingsScreenViewModel : ScreenViewModelBase
    {
        private readonly SettingsService _settingsService;

        public SettingsScreenViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public override ScreenId Id => ScreenId.Settings;

        public ReactiveProperty<string> CurrentLocale => _settingsService.Locale;
        public ReactiveProperty<bool> Fullscreen => _settingsService.Fullscreen;

        public void SelectLocale(string code) => _settingsService.SetLocale(code);
        public void SetFullscreen(bool value) => _settingsService.SetFullscreen(value);

        protected override void OnOpen(object args) { }
        protected override void OnClose() { }
    }
}

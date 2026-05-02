using Internal.Scripts.Save;
using R3;
using UnityEngine.Localization.Settings;
using Zenject;

namespace Internal.Scripts.Settings
{
    public class SettingsService : IInitializable
    {
        private const string FILE_NAME = "settings.json";

        private readonly IJsonStorage _storage;
        private SettingsData _data;

        public ReactiveProperty<string> Locale { get; } = new("ru");
        public ReactiveProperty<bool> Fullscreen { get; } = new(true);

        public SettingsService(IJsonStorage storage)
        {
            _storage = storage;
        }

        public void Initialize()
        {
            _data = _storage.Load<SettingsData>(FILE_NAME) ?? new SettingsData();
            ApplyLocale(_data.LocaleCode);
            ApplyFullscreen(_data.Fullscreen);
        }

        public void SetLocale(string code)
        {
            _data.LocaleCode = code;
            ApplyLocale(code);
            Save();
        }

        public void SetFullscreen(bool value)
        {
            _data.Fullscreen = value;
            ApplyFullscreen(value);
            Save();
        }

        private void ApplyLocale(string code)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(code);
            if (locale == null)
            {
                locale = LocalizationSettings.AvailableLocales.GetLocale("ru");
                code = "ru";
            }

            if (locale != null)
                LocalizationSettings.SelectedLocale = locale;
            Locale.Value = code;
        }

        private void ApplyFullscreen(bool value)
        {
            UnityEngine.Screen.fullScreen = value;
            Fullscreen.Value = value;
        }

        private void Save()
        {
            _storage.Save(FILE_NAME, _data, false);
        }
    }
}

using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.Trader
{
    public sealed class TraderScreenViewModel : ScreenViewModelBase
    {
        private readonly PlayerSkillRepository _skillRepository;
        private readonly PlayerLanguageRepository _languageRepository;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly CaravanDatabase _caravanDb;
        private readonly LanguagePriceModifier _languagePriceModifier;
        private readonly GameBalanceConfig _config;
        private readonly TraderUICatalog _catalog;
        private readonly UiThemeService _themeService;
        private readonly ReactiveProperty<TraderViewState> _state = new();

        public TraderScreenViewModel(
            PlayerSkillRepository skillRepository,
            PlayerLanguageRepository languageRepository,
            PlayerResourceRepository resourceRepository,
            CaravanDatabase caravanDb,
            LanguagePriceModifier languagePriceModifier,
            GameBalanceConfig config,
            TraderUICatalog catalog,
            UiThemeService themeService)
        {
            _skillRepository = skillRepository;
            _languageRepository = languageRepository;
            _resourceRepository = resourceRepository;
            _caravanDb = caravanDb;
            _languagePriceModifier = languagePriceModifier;
            _config = config;
            _catalog = catalog;
            _themeService = themeService;
        }

        public UiThemeService ThemeService => _themeService;

        public override ScreenId Id => ScreenId.Trader;
        public Observable<TraderViewState> State => _state;

        protected override void OnOpen(object args)
        {
            BuildState();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        protected override void OnClose()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(Locale _) => BuildState();

        private void BuildState()
        {
            var profiles = BuildProfiles();
            var skills = BuildSkills();
            var languages = BuildLanguages();
            _state.Value = new TraderViewState(profiles, skills, languages);
        }

        private List<ProfileEntry> BuildProfiles()
        {
            var list = new List<ProfileEntry>();
            CartClassData cartClass = _caravanDb.GetCartClassById(_resourceRepository.Current.CartClassId);

            foreach (var item in _catalog.ProfileItems)
            {
                string value = "";
                if (item.Id == "cart" && cartClass != null)
                {
                    string cartName = LocalizationService.ResolveString(cartClass.Name, cartClass.Id, "Trader.CartName");
                    string cartDesc = LocalizationService.ResolveString(cartClass.Description, cartClass.Id, "Trader.CartDesc");
                    value = $"{cartName}\n{cartDesc}";
                }
                list.Add(new ProfileEntry(item.Header, value));
            }

            return list;
        }

        private List<SkillViewData> BuildSkills()
        {
            var list = new List<SkillViewData>();
            int maxSkill = _config.MaxSkill;
            PlayerSkillState current = _skillRepository.Current;

            foreach (SkillType type in Enum.GetValues(typeof(SkillType)))
            {
                if (type == SkillType.None)
                    continue;

                if (!_catalog.TryGetSkill(type, out var name, out var desc))
                    continue;

                int value = current.GetSkill(type);
                float progress = maxSkill > 0 ? (float)value / maxSkill : 0f;

                list.Add(new SkillViewData(name, desc, progress, value.ToString()));
            }

            return list;
        }

        private List<LanguageViewData> BuildLanguages()
        {
            var list = new List<LanguageViewData>();
            PlayerLanguageState current = _languageRepository.Current;

            foreach (LanguageType type in Enum.GetValues(typeof(LanguageType)))
            {
                if (type == LanguageType.None)
                    continue;

                if (!_catalog.TryGetLanguage(type, out var name, out var desc))
                    continue;

                LanguageProficiency playerLevel = current.GetProficiency(type);
                LanguageProficiency effective = _languagePriceModifier.GetEffectiveProficiency(type);

                LocalizedString value;
                if (effective > playerLevel)
                {
                    value = new LocalizedString("UI", $"UI.Language.Proficiency.{effective}");
                    list.Add(new LanguageViewData(name, desc, effective, value));
                }
                else
                {
                    value = new LocalizedString("UI", $"UI.Language.Proficiency.{playerLevel}");
                    list.Add(new LanguageViewData(name, desc, playerLevel, value));
                }
            }

            return list;
        }
    }
}

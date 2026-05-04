using System;
using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Player;
using Internal.Scripts.Events;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screens.Trader;
using Internal.Scripts.UI.Theme;
using R3;

namespace Internal.Scripts.UI.Screens.LanguageSchool
{
    public sealed class LanguageSchoolScreenViewModel : ScreenViewModelBase
    {
        private readonly PlayerLanguageRepository _languageRepository;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly EconomyDatabase _economyDatabase;
        private readonly GameBalanceConfig _config;
        private readonly DayTracker _dayTracker;
        private readonly TraderUICatalog _catalog;
        private readonly UiThemeService _themeService;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly FoodConsumptionCalculator _foodCalculator;
        private readonly ReactiveProperty<LanguageSchoolViewState> _state = new();

        private string _cityId;

        public UiThemeService ThemeService => _themeService;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;

        public LanguageSchoolScreenViewModel(
            PlayerLanguageRepository languageRepository,
            PlayerResourceRepository resourceRepository,
            EconomyDatabase economyDatabase,
            GameBalanceConfig config,
            DayTracker dayTracker,
            TraderUICatalog catalog,
            UiThemeService themeService,
            ResourceIconCatalog resourceIcons,
            FoodConsumptionCalculator foodCalculator)
        {
            _languageRepository = languageRepository;
            _resourceRepository = resourceRepository;
            _economyDatabase = economyDatabase;
            _config = config;
            _dayTracker = dayTracker;
            _catalog = catalog;
            _themeService = themeService;
            _resourceIcons = resourceIcons;
            _foodCalculator = foodCalculator;
        }

        public event Action<LanguageType, LanguageProficiency> OnLanguageStudied;

        public override ScreenId Id => ScreenId.LanguageSchool;
        public Observable<LanguageSchoolViewState> State => _state;

        protected override void OnOpen(object args)
        {
            _cityId = args as string;
            BuildState();
        }

        protected override void OnClose()
        {
        }

        public void LearnLanguage(LanguageType language)
        {
            var current = _languageRepository.Current.GetProficiency(language);
            var (cost, days) = GetCostAndDays(current);
            if (cost <= 0) return;
            if (_resourceRepository.Current.Money < cost) return;

            var nextLevel = NextLevel(current);

            _resourceRepository.UpdateResources(s => s.Money -= cost);
            _dayTracker.AdvanceDays(days);
            _languageRepository.SetLanguage(language, nextLevel);
            OnLanguageStudied?.Invoke(language, nextLevel);

            BuildState();
        }

        private void BuildState()
        {
            var languages = _economyDatabase.GetLanguagesForCity(_cityId);
            int money = _resourceRepository.Current.Money;
            float totalFoodPerDay = GetTotalFoodPerDay();
            var entries = new List<LanguageSchoolEntry>(languages.Count);

            foreach (var lang in languages)
            {
                var current = _languageRepository.Current.GetProficiency(lang);
                var (cost, days) = GetCostAndDays(current);
                string name = _catalog.GetLanguageName(lang);

                bool canLearn = cost > 0 && money >= cost;
                var nextLevel = cost > 0 ? NextLevel(current) : current;

                string currentFormatted = FormatLevel(LocUI.UI_LanguageSchool_Item_Text_CurrentLevel, current);
                string nextFormatted = FormatLevel(LocUI.UI_LanguageSchool_Item_Text_NextLevel, nextLevel);
                string daysFormatted = FormatDays(days, totalFoodPerDay, cost);
                string learnText = FormatLearnButton(cost);

                entries.Add(new LanguageSchoolEntry(
                    lang, name, current, nextLevel, cost, days, canLearn,
                    currentFormatted, nextFormatted, daysFormatted, learnText));
            }

            int food = (int)_resourceRepository.Current.Food;
            _state.Value = new LanguageSchoolViewState(entries, money, food);
        }

        private string FormatLevel(string locKey, LanguageProficiency level)
        {
            string levelName = ResolveProficiencyName(level);
            string effect = FormatEffect(level);
            return LocalizationService.Resolve(LocUI.Table, locKey, null, levelName, effect);
        }

        private string FormatEffect(LanguageProficiency level)
        {
            float buy = LanguagePriceModifier.GetBuyMod(level);
            float sell = LanguagePriceModifier.GetSellMod(level);
            string buyStr = FormatPercent(buy);
            string sellStr = FormatPercent(sell);
            return LocalizationService.Resolve(LocUI.Table, LocUI.UI_LanguageSchool_Item_Text_BuySellEffect, null, buyStr, sellStr);
        }

        private string FormatDays(int days, float foodPerDay, int cost)
        {
            if (days <= 0)
                return LocalizationService.Resolve(LocUI.Table, LocUI.UI_LanguageSchool_Main_Text_MaxLevel);

            int supplyCost = (int)(days * foodPerDay);
            return LocalizationService.Resolve(LocUI.Table, LocUI.UI_LanguageSchool_Item_Text_DaysToLearn, null, days, supplyCost, cost);
        }

        private string FormatLearnButton(int cost)
        {
            if (cost <= 0)
                return LocalizationService.Resolve(LocUI.Table, LocUI.UI_LanguageSchool_Main_Text_MaxLevel);

            return LocalizationService.Resolve(LocUI.Table, LocUI.UI_LanguageSchool_Item_Button_LearnFor, null, cost);
        }

        private static string ResolveProficiencyName(LanguageProficiency level)
        {
            return TraderUICatalog.GetProficiencyName(level);
        }

        private static string FormatPercent(float value)
        {
            int pct = (int)(value * 100f);
            return pct >= 0 ? $"+{pct}%" : $"{pct}%";
        }

        private float GetTotalFoodPerDay() => _foodCalculator.Calculate(_resourceRepository.Current);

        private static LanguageProficiency NextLevel(LanguageProficiency current)
        {
            var next = (LanguageProficiency)((int)current + 1);
            return Enum.IsDefined(typeof(LanguageProficiency), next) ? next : current;
        }

        private (int cost, int days) GetCostAndDays(LanguageProficiency current)
        {
            return current switch
            {
                LanguageProficiency.None => (_config.SchoolCostBasic, _config.SchoolDaysBasic),
                LanguageProficiency.Basic => (_config.SchoolCostConversational, _config.SchoolDaysConversational),
                LanguageProficiency.Conversational => (_config.SchoolCostFluent, _config.SchoolDaysFluent),
                _ => (0, 0)
            };
        }
    }
}

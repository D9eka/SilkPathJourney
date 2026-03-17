using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Npc.Names;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Tavern
{
    public sealed class TavernScreenViewModel : ScreenViewModelBase
    {
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly CaravanDatabase _caravanDb;
        private readonly CompanionService _companionService;
        private readonly EconomyDatabase _economyDb;
        private readonly EventTrigger _eventTrigger;
        private readonly EventDatabase _eventDb;
        private readonly NameDatabase _nameDb;
        private readonly UiThemeService _themeService;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly ReactiveProperty<TavernViewState> _state = new();

        private string _cityId;
        private CultureId _cityCulture;
        private readonly List<(CompanionType type, CompanionQuality quality)> _availableSlots = new();

        public UiThemeService ThemeService => _themeService;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;
        public Observable<TavernViewState> State => _state;
        public override ScreenId Id => ScreenId.Tavern;

        public TavernScreenViewModel(
            PlayerResourceRepository resourceRepository,
            CaravanDatabase caravanDb,
            CompanionService companionService,
            EconomyDatabase economyDb,
            EventTrigger eventTrigger,
            EventDatabase eventDb,
            NameDatabase nameDb,
            UiThemeService themeService,
            ResourceIconCatalog resourceIcons)
        {
            _resourceRepository = resourceRepository;
            _caravanDb = caravanDb;
            _companionService = companionService;
            _economyDb = economyDb;
            _eventTrigger = eventTrigger;
            _eventDb = eventDb;
            _nameDb = nameDb;
            _themeService = themeService;
            _resourceIcons = resourceIcons;
        }

        protected override void OnOpen(object args)
        {
            _cityId = args as string;
            _cityCulture = ResolveCityCulture(_cityId);
            BuildState();
        }

        protected override void OnClose()
        {
        }

        public void HireCompanion(int index)
        {
            if (index < 0 || index >= _availableSlots.Count)
                return;

            var (type, quality) = _availableSlots[index];
            if (!_companionService.HireCompanion(type, quality, _cityCulture))
                return;

            BuildState();
        }

        public void TalkToQuestGiver(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return;

            var eventData = _eventDb.GetById(eventId);
            if (eventData != null)
                _eventTrigger.TriggerEvent(eventData);
        }

        private void BuildState()
        {
            var resources = _resourceRepository.Current;
            int money = resources.Money;
            int currentCount = resources.Companions?.Count ?? 0;
            int maxCompanions = resources.MaxCompanions;

            _availableSlots.Clear();
            var hireList = new List<CompanionHireData>();
            int index = 0;

            string effectPrefix = ResolveLoc("UI.Global.Effect.Prefix", "UI.Global.Effect.Prefix");

            foreach (var typeData in _caravanDb.CompanionTypes)
            {
                foreach (var qualityEntry in _caravanDb.CompanionQualities)
                {
                    int hireCost = Mathf.RoundToInt(typeData.HireCostBase * qualityEntry.PriceMultiplier);
                    int dailyCost = Mathf.RoundToInt(typeData.DailyCostBase * qualityEntry.DailyCostMultiplier);

                    string typeName = LocalizationService.ResolveString(
                        typeData.Name, typeData.Id, "Tavern.CompanionType");
                    string levelPrefix = ResolveLoc("UI.Global.Level.Prefix", "UI.Global.Level.Prefix");
                    string rawQuality = LocalizationService.ResolveString(
                        qualityEntry.Name, qualityEntry.Quality.ToString(), "Tavern.CompanionQuality");
                    string qualityName = $"{levelPrefix} {rawQuality}";

                    var nameEntry = _nameDb.GetRandom(_cityCulture);
                    string companionName = nameEntry?.Name != null
                        ? LocalizationService.ResolveString(nameEntry.Name, nameEntry.Id, "Tavern.Name")
                        : "";
                    string displayName = string.IsNullOrEmpty(companionName)
                        ? typeName
                        : $"{typeName} {companionName}";

                    var bonus = _caravanDb.GetCompanionBonus(typeData.Type, qualityEntry.Quality);
                    string rawEffect;
                    if (typeData.Type == CompanionType.Translator)
                    {
                        LanguageType cityLang = ResolveCityLanguage(_cityCulture);
                        string langName = ResolveLanguageName(cityLang);
                        int profValue = Mathf.RoundToInt(bonus.BonusValue);
                        string profName = ResolveProficiencyName((LanguageProficiency)profValue);
                        rawEffect = ResolveLoc("UI.Translator.LanguageEffect",
                            "UI.Translator.LanguageEffect", langName, profName);
                    }
                    else
                    {
                        rawEffect = bonus.Description != null
                            ? LocalizationService.ResolveString(bonus.Description, bonus.BonusKey, "Tavern.Bonus")
                            : bonus.BonusKey ?? "";
                    }
                    string effectText = $"{effectPrefix} {rawEffect}";

                    bool canHire = money >= hireCost && currentCount < maxCompanions;

                    string hireCostText = ResolveLoc("UI.Tavern.HireCost", "UI.Tavern.HireCost", hireCost);
                    string dailyCostText = ResolveLoc("UI.Tavern.DailyCost", "UI.Tavern.DailyCost", dailyCost);

                    _availableSlots.Add((typeData.Type, qualityEntry.Quality));
                    hireList.Add(new CompanionHireData(
                        displayName, qualityName, effectText,
                        hireCostText, dailyCostText, canHire,
                        index, typeData.Type, qualityEntry.Quality));

                    index++;
                }
            }

            string slotsFormatted = ResolveLoc("UI.Tavern.SlotsAvailable", "UI.Tavern.SlotsAvailable", currentCount, maxCompanions);
            string rumorsText = "";
            var roadInfos = new List<RoadInfoEntry>();

            _state.Value = new TavernViewState(
                money, rumorsText, roadInfos, hireList,
                currentCount, maxCompanions, slotsFormatted,
                false, null, null);
        }

        public void BuyRoadInfo(int index)
        {
        }

        private CultureId ResolveCityCulture(string cityId)
        {
            if (string.IsNullOrEmpty(cityId))
                return CultureId.None;

            var city = _economyDb.Cities.Find(c =>
                string.Equals(c.Id, cityId, StringComparison.OrdinalIgnoreCase));
            return city != null ? city.PrimaryCulture : CultureId.None;
        }

        private LanguageType ResolveCityLanguage(CultureId culture)
        {
            if (culture == CultureId.None)
                return LanguageType.None;

            foreach (var mapping in _economyDb.CultureLanguages)
            {
                if (mapping.Culture == culture)
                    return mapping.Language;
            }

            return LanguageType.None;
        }

        private static string ResolveLanguageName(LanguageType lang)
        {
            if (lang == LanguageType.None)
                return "";

            var localized = new LocalizedString("UI", $"UI.Language.{lang}.Name");
            return LocalizationService.ResolveString(localized, lang.ToString(), $"LanguageName.{lang}");
        }

        private static string ResolveProficiencyName(LanguageProficiency proficiency)
        {
            var localized = new LocalizedString("UI", $"UI.Language.Proficiency.{proficiency}");
            return LocalizationService.ResolveString(localized, proficiency.ToString(), $"Proficiency.{proficiency}");
        }

        private static string ResolveLoc(string key, string fallback, params object[] args)
        {
            var localized = new LocalizedString("UI", key);
            return LocalizationService.ResolveString(localized, fallback, key, args);
        }

    }
}

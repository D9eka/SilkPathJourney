using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Cities.Rumors;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Npc.Names;
using Internal.Scripts.Player;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Tavern
{
    public sealed class TavernScreenViewModel : ScreenViewModelBase
    {
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly CompanionService _companionService;
        private readonly EconomyDatabase _economyDb;
        private readonly EventTrigger _eventTrigger;
        private readonly EventDatabase _eventDb;
        private readonly UiThemeService _themeService;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly RumorService _rumorService;
        private readonly RumorFormatter _rumorFormatter = new();
        private readonly CompanionHireFormatter _hireFormatter;
        private readonly ReactiveProperty<TavernViewState> _state = new();

        private string _cityId;
        private CultureId _cityCulture;
        private readonly List<(CompanionType type, CompanionQuality quality)> _availableSlots = new();
        private List<RumorData> _currentRumors = new();

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
            ResourceIconCatalog resourceIcons,
            RumorService rumorService)
        {
            _resourceRepository = resourceRepository;
            _companionService = companionService;
            _economyDb = economyDb;
            _eventTrigger = eventTrigger;
            _eventDb = eventDb;
            _themeService = themeService;
            _resourceIcons = resourceIcons;
            _rumorService = rumorService;
            _hireFormatter = new CompanionHireFormatter(caravanDb, economyDb, nameDb, companionService);
        }

        protected override void OnOpen(object args)
        {
            _cityId = args as string;
            var city = _economyDb.Cities.Find(c =>
                string.Equals(c.Id, _cityId, StringComparison.OrdinalIgnoreCase));
            _cityCulture = city?.PrimaryCulture ?? CultureId.None;
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
            int cost = _companionService.GetHireCost(type, quality);
            if (!_companionService.HireCompanion(type, quality, cost, _cityCulture))
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

            var hireList = _hireFormatter.Build(_cityCulture, money, currentCount, maxCompanions, _availableSlots);

            string slotsFormatted = ResolveLoc("UI.Tavern.SlotsAvailable", "UI.Tavern.SlotsAvailable", currentCount, maxCompanions);
            _currentRumors = _rumorService.GetAvailableRumors(_cityId);
            int rumorCost = _rumorService.GetRumorCost();
            string rumorsText = _rumorFormatter.FormatRumorsText(_currentRumors);
            var roadInfos = _rumorFormatter.BuildRoadInfoEntries(_currentRumors, money, rumorCost);

            _state.Value = new TavernViewState(
                money, rumorsText, roadInfos, hireList,
                currentCount, maxCompanions, slotsFormatted,
                false, null, null);
        }

        public void BuyRoadInfo(int index)
        {
            if (index < 0 || index >= _currentRumors.Count)
                return;

            int cost = _rumorService.GetRumorCost();
            if (_resourceRepository.Current.Money < cost)
                return;

            _resourceRepository.UpdateResources(s => s.Money -= cost);
            _rumorService.PurchaseRumors(_currentRumors[index].City.Id);
            BuildState();
        }

        private static string ResolveLoc(string key, string fallback, params object[] args)
        {
            var localized = new LocalizedString("UI", key);
            return LocalizationService.ResolveString(localized, fallback, key, args);
        }
    }
}

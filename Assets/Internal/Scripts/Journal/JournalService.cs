using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Buildings.Healer;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.Quests;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.Save;
using Internal.Scripts.Trading;
using Internal.Scripts.Travel.Triggers;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Caravansary;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.LanguageSchool;
using Internal.Scripts.UI.Screens.Tavern;
using Internal.Scripts.UI.Screens.Trader;
using Internal.Scripts.UI.StackService;
using Zenject;

namespace Internal.Scripts.Journal
{
    public class JournalService : IInitializable, IDisposable
    {
        private readonly QuestRepository _questRepository;
        private readonly EventCloseSignal _closeSignal;
        private readonly DayTracker _dayTracker;
        private readonly SaveRepository _saveRepository;
        private readonly QuestDatabase _questDatabase;
        private readonly ICityEntryService _cityEntryService;
        private readonly CompanionInjuredApplier _companionInjuredApplier;
        private readonly CompanionService _companionService;
        private readonly HealerService _healerService;
        private readonly TradeModel _tradeModel;
        private readonly DistanceTrackingService _distanceTracking;
        private readonly GameBalanceConfig _balance;
        private readonly EconomyDatabase _economyDatabase;
        private readonly ScreenStackService _screenStack;
        private readonly TraderUICatalog _traderCatalog;

        private float _lastDistanceUnits;

        public event Action OnEntryAdded;

        public JournalService(
            QuestRepository questRepository,
            EventCloseSignal closeSignal,
            DayTracker dayTracker,
            SaveRepository saveRepository,
            QuestDatabase questDatabase,
            ICityEntryService cityEntryService,
            CompanionInjuredApplier companionInjuredApplier,
            CompanionService companionService,
            HealerService healerService,
            TradeModel tradeModel,
            DistanceTrackingService distanceTracking,
            GameBalanceConfig balance,
            EconomyDatabase economyDatabase,
            ScreenStackService screenStack,
            TraderUICatalog traderCatalog)
        {
            _questRepository = questRepository;
            _closeSignal = closeSignal;
            _dayTracker = dayTracker;
            _saveRepository = saveRepository;
            _questDatabase = questDatabase;
            _cityEntryService = cityEntryService;
            _companionInjuredApplier = companionInjuredApplier;
            _companionService = companionService;
            _healerService = healerService;
            _tradeModel = tradeModel;
            _distanceTracking = distanceTracking;
            _balance = balance;
            _economyDatabase = economyDatabase;
            _screenStack = screenStack;
            _traderCatalog = traderCatalog;
        }

        public IReadOnlyList<JournalEntry> Entries => _saveRepository.Data.Journal;

        public void Initialize()
        {
            _questRepository.QuestStarted += OnQuestStarted;
            _questRepository.QuestAdvanced += OnQuestAdvanced;
            _questRepository.QuestCompleted += OnQuestCompleted;
            _questRepository.QuestFailed += OnQuestFailed;
            _closeSignal.Closed += OnEventClosed;
            _cityEntryService.OnCityEntered += OnCityEntered;
            _cityEntryService.OnCityLeft += OnCityLeft;
            _companionInjuredApplier.OnCompanionInjured += OnCompanionInjured;
            _companionService.OnDismissed += OnCompanionDismissed;
            _healerService.OnHealed += OnCompanionHealed;
            _tradeModel.OnTradeCompleted += OnTradeCompleted;
            _dayTracker.OnDayChanged += OnDayChanged;
            _screenStack.OnScreenOpened += OnScreenOpened;
        }

        public void Dispose()
        {
            _questRepository.QuestStarted -= OnQuestStarted;
            _questRepository.QuestAdvanced -= OnQuestAdvanced;
            _questRepository.QuestCompleted -= OnQuestCompleted;
            _questRepository.QuestFailed -= OnQuestFailed;
            _closeSignal.Closed -= OnEventClosed;
            _cityEntryService.OnCityEntered -= OnCityEntered;
            _cityEntryService.OnCityLeft -= OnCityLeft;
            _companionInjuredApplier.OnCompanionInjured -= OnCompanionInjured;
            _companionService.OnDismissed -= OnCompanionDismissed;
            _healerService.OnHealed -= OnCompanionHealed;
            _tradeModel.OnTradeCompleted -= OnTradeCompleted;
            _dayTracker.OnDayChanged -= OnDayChanged;
            _screenStack.OnScreenOpened -= OnScreenOpened;

            UnsubscribeScreenVMs();
        }

        public IEnumerable<int> GetDays() =>
            Entries.Select(e => e.Day).Distinct().OrderByDescending(d => d);

        public IEnumerable<JournalEntry> GetEntriesForDay(int day) =>
            Entries.Where(e => e.Day == day);

        public IEnumerable<JournalEntry> GetEntriesForQuest(string questId) =>
            Entries.Where(e => e.RelatedQuestId == questId);

        private void OnQuestStarted(string questId) =>
            Append(JournalEntryType.QuestStarted, "ui.journal.entry.quest_started", questId, ResolveQuestName(questId));

        private void OnQuestAdvanced(string questId) =>
            Append(JournalEntryType.QuestAdvanced, "ui.journal.entry.quest_advanced", questId, ResolveQuestName(questId));

        private void OnQuestCompleted(string questId) =>
            Append(JournalEntryType.QuestCompleted, "ui.journal.entry.quest_completed", questId, ResolveQuestName(questId));

        private void OnQuestFailed(string questId) =>
            Append(JournalEntryType.QuestFailed, "ui.journal.entry.quest_failed", questId, ResolveQuestName(questId));

        private void OnEventClosed(EventData ev)
        {
            if (ev == null) return;
            string name = LocalizationService.ResolveString(ev.Name, ev.Id, "JournalService.EventName");
            bool isCrisis = ev.Category >= EventCategory.Crisis && ev.Category <= EventCategory.CrisisMoney;
            JournalEntryType type = isCrisis ? JournalEntryType.Crisis : JournalEntryType.Event;
            string key = isCrisis ? "ui.journal.entry.crisis" : "ui.journal.entry.event";
            Append(type, key, null, name);
        }

        private void OnCityEntered(CityData city)
        {
            string name = LocalizationService.ResolveString(city.Name, city.Id, "JournalService.CityName");
            Append(JournalEntryType.Arrival, "ui.journal.entry.arrival", null, name);
        }

        private void OnCityLeft(CityData city)
        {
            string name = LocalizationService.ResolveString(city.Name, city.Id, "JournalService.CityName");
            Append(JournalEntryType.Departure, "ui.journal.entry.departure", null, name);
        }

        private void OnCompanionInjured(string companionName)
        {
            Append(JournalEntryType.Loss, "ui.journal.entry.loss", null, companionName);
        }

        private void OnCompanionDismissed(CompanionState companion)
        {
            Append(JournalEntryType.CompanionDismissed, "ui.journal.entry.companion_dismissed", null, companion.Name);
        }

        private void OnCompanionHealed(CompanionState companion, int cost)
        {
            Append(JournalEntryType.CompanionHealed, "ui.journal.entry.companion_healed", null, companion.Name);
        }

        private void OnTradeCompleted(TradeModel.TradeResult result)
        {
            if (result.ItemDeltas == null || result.ItemDeltas.Count == 0)
                return;

            string cityName = ResolveCityName(result.CityId);
            Append(JournalEntryType.Trade, "ui.journal.entry.trade", null, cityName);
        }

        private void OnDayChanged(int day)
        {
            float current = _distanceTracking.TotalDistanceUnits;
            float delta = current - _lastDistanceUnits;
            if (delta > 0 && _balance.WorldUnitsPerKm > 0)
            {
                float km = delta / _balance.WorldUnitsPerKm;
                Append(JournalEntryType.Travel, "ui.journal.entry.travel", null,
                    km.ToString("F1"));
            }
            _lastDistanceUnits = current;
        }

        private CaravansaryScreenViewModel _caravansaryVM;
        private LanguageSchoolScreenViewModel _languageSchoolVM;
        private TavernScreenViewModel _tavernVM;

        private void OnScreenOpened(ScreenId id)
        {
            switch (id)
            {
                case ScreenId.Caravansary:
                    var caravansaryVM = _screenStack.GetViewModel<CaravansaryScreenViewModel>(ScreenId.Caravansary);
                    if (caravansaryVM != null && caravansaryVM != _caravansaryVM)
                    {
                        if (_caravansaryVM != null)
                            _caravansaryVM.OnPurchased -= OnCaravansaryPurchased;
                        _caravansaryVM = caravansaryVM;
                        _caravansaryVM.OnPurchased += OnCaravansaryPurchased;
                    }
                    break;

                case ScreenId.LanguageSchool:
                    var langVM = _screenStack.GetViewModel<LanguageSchoolScreenViewModel>(ScreenId.LanguageSchool);
                    if (langVM != null && langVM != _languageSchoolVM)
                    {
                        if (_languageSchoolVM != null)
                            _languageSchoolVM.OnLanguageStudied -= OnLanguageStudied;
                        _languageSchoolVM = langVM;
                        _languageSchoolVM.OnLanguageStudied += OnLanguageStudied;
                    }
                    break;

                case ScreenId.Tavern:
                    var tavernVM = _screenStack.GetViewModel<TavernScreenViewModel>(ScreenId.Tavern);
                    if (tavernVM != null && tavernVM != _tavernVM)
                    {
                        if (_tavernVM != null)
                            _tavernVM.OnCompanionHiredInCity -= OnTavernHired;
                        _tavernVM = tavernVM;
                        _tavernVM.OnCompanionHiredInCity += OnTavernHired;
                    }
                    break;
            }
        }

        private void UnsubscribeScreenVMs()
        {
            if (_caravansaryVM != null)
                _caravansaryVM.OnPurchased -= OnCaravansaryPurchased;
            if (_languageSchoolVM != null)
                _languageSchoolVM.OnLanguageStudied -= OnLanguageStudied;
            if (_tavernVM != null)
                _tavernVM.OnCompanionHiredInCity -= OnTavernHired;
        }

        private void OnCaravansaryPurchased(string cityId, string description)
        {
            string cityName = ResolveCityName(cityId);
            Append(JournalEntryType.Caravansary, "ui.journal.entry.caravansary", null,
                cityName, description);
        }

        private void OnLanguageStudied(LanguageType language, LanguageProficiency proficiency)
        {
            string cityId = _cityEntryService.CurrentCity?.Id;
            string cityName = cityId != null ? ResolveCityName(cityId) : string.Empty;
            string langName = _traderCatalog.GetLanguageName(language);
            Append(JournalEntryType.Teacher, "ui.journal.entry.teacher", null,
                cityName, langName);
        }

        private void OnTavernHired(string cityId, string companionName)
        {
            string cityName = ResolveCityName(cityId);
            Append(JournalEntryType.Tavern, "ui.journal.entry.tavern", null,
                cityName, companionName);
        }

        private string ResolveCityName(string cityId)
        {
            if (string.IsNullOrEmpty(cityId)) return cityId ?? string.Empty;
            var city = _economyDatabase.Cities.Find(c =>
                string.Equals(c.Id, cityId, StringComparison.OrdinalIgnoreCase));
            if (city == null) return cityId;
            return LocalizationService.ResolveString(city.Name, city.Id, "JournalService.CityName");
        }

        private void Append(JournalEntryType type, string titleKey, string questId, params string[] args)
        {
            var entry = new JournalEntry
            {
                Day = _dayTracker.CurrentDay,
                Type = type,
                TitleKey = titleKey,
                Args = args,
                RelatedQuestId = questId
            };

            _saveRepository.Data.Journal.Add(entry);
            _saveRepository.Save();
            OnEntryAdded?.Invoke();
        }

        private string ResolveQuestName(string questId)
        {
            var quest = _questDatabase.GetById(questId);
            if (quest == null) return questId;
            return LocalizationService.ResolveString(quest.Name, quest.Id, "JournalService.QuestName");
        }
    }
}

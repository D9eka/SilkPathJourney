using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.Quests;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.Save;
using Internal.Scripts.UI.Localization;
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

        public event Action OnEntryAdded;

        public JournalService(
            QuestRepository questRepository,
            EventCloseSignal closeSignal,
            DayTracker dayTracker,
            SaveRepository saveRepository,
            QuestDatabase questDatabase,
            ICityEntryService cityEntryService,
            CompanionInjuredApplier companionInjuredApplier)
        {
            _questRepository = questRepository;
            _closeSignal = closeSignal;
            _dayTracker = dayTracker;
            _saveRepository = saveRepository;
            _questDatabase = questDatabase;
            _cityEntryService = cityEntryService;
            _companionInjuredApplier = companionInjuredApplier;
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
            _companionInjuredApplier.OnCompanionInjured += OnCompanionInjured;
        }

        public void Dispose()
        {
            _questRepository.QuestStarted -= OnQuestStarted;
            _questRepository.QuestAdvanced -= OnQuestAdvanced;
            _questRepository.QuestCompleted -= OnQuestCompleted;
            _questRepository.QuestFailed -= OnQuestFailed;
            _closeSignal.Closed -= OnEventClosed;
            _cityEntryService.OnCityEntered -= OnCityEntered;
            _companionInjuredApplier.OnCompanionInjured -= OnCompanionInjured;
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

        private void OnCompanionInjured(string companionName)
        {
            Append(JournalEntryType.Loss, "ui.journal.entry.loss", null, companionName);
        }

        private void Append(JournalEntryType type, string titleKey, string questId, string argName)
        {
            var entry = new JournalEntry
            {
                Day = _dayTracker.CurrentDay,
                Type = type,
                TitleKey = titleKey,
                Args = new[] { argName },
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

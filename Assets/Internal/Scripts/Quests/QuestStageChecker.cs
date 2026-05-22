using System;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Inventory;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.Quests.Generated;
using Internal.Scripts.Quests.Save;
using Internal.Scripts.World.State;
using Zenject;

namespace Internal.Scripts.Quests
{
    public class QuestStageChecker : IInitializable, IDisposable
    {
        private readonly QuestRepository _questRepository;
        private readonly QuestDatabase _questDatabase;
        private readonly InventoryRepository _inventoryRepository;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly DayTracker _dayTracker;
        private readonly ICityEntryService _cityEntryService;
        private readonly QuestRewardApplier _rewardApplier;
        private readonly EventDatabase _eventDatabase;
        private readonly EventSelector _eventSelector;
        private readonly EventTrigger _eventTrigger;
        private readonly EventCloseSignal _closeSignal;

        public QuestStageChecker(
            QuestRepository questRepository,
            QuestDatabase questDatabase,
            InventoryRepository inventoryRepository,
            PlayerResourceRepository resourceRepository,
            DayTracker dayTracker,
            ICityEntryService cityEntryService,
            QuestRewardApplier rewardApplier,
            EventDatabase eventDatabase,
            EventSelector eventSelector,
            EventTrigger eventTrigger,
            EventCloseSignal closeSignal)
        {
            _questRepository = questRepository;
            _questDatabase = questDatabase;
            _inventoryRepository = inventoryRepository;
            _resourceRepository = resourceRepository;
            _dayTracker = dayTracker;
            _cityEntryService = cityEntryService;
            _rewardApplier = rewardApplier;
            _eventDatabase = eventDatabase;
            _eventSelector = eventSelector;
            _eventTrigger = eventTrigger;
            _closeSignal = closeSignal;
        }

        public void Initialize()
        {
            _dayTracker.OnDayChanged += HandleDayChanged;
            _cityEntryService.OnCityEntered += HandleCityEntered;
            _questRepository.QuestStarted += HandleQuestStarted;
            _questRepository.QuestAdvanced += HandleQuestChanged;
            _closeSignal.Closed += HandleEventClosed;
        }

        public void Dispose()
        {
            _dayTracker.OnDayChanged -= HandleDayChanged;
            _cityEntryService.OnCityEntered -= HandleCityEntered;
            _questRepository.QuestStarted -= HandleQuestStarted;
            _questRepository.QuestAdvanced -= HandleQuestChanged;
            _closeSignal.Closed -= HandleEventClosed;
        }

        private void HandleEventClosed(EventData _) => CheckAll();

        private void HandleDayChanged(int _) => CheckAll();
        private void HandleCityEntered(CityData _) => CheckAll();
        private void HandleQuestStarted(string _) => CheckAutoCompleteStages(_resourceRepository.Current);
        private void HandleQuestChanged(string _) => CheckAll();

        private bool _checking;

        private void CheckAll()
        {
            if (_checking) return;
            _checking = true;
            try
            {
                PlayerResourceState resources = _resourceRepository.Current;
                CheckAutoCompleteStages(resources);
                CheckTriggerEvents(resources);
                CheckFailConditions(resources);
            }
            finally
            {
                _checking = false;
            }
        }

        private void CheckAutoCompleteStages(PlayerResourceState resources)
        {
            bool advanced;
            do
            {
                advanced = false;
                var activeQuests = _questRepository.GetActiveQuests();
                for (int i = activeQuests.Count - 1; i >= 0; i--)
                {
                    QuestStateEntry entry = activeQuests[i];
                    QuestData quest = _questDatabase.GetById(entry.QuestId);
                    if (quest?.Stages == null) continue;
                    if (entry.CurrentStageIndex >= quest.Stages.Count) continue;

                    QuestStageData stage = quest.Stages[entry.CurrentStageIndex];
                    QuestStageCondition condition = stage.AutoCompleteCondition;
                    if (condition.Type == QuestStageConditionType.None) continue;

                    if (EvaluateStageCondition(condition, entry))
                    {
                        _questRepository.AdvanceQuest(entry.QuestId);
                        advanced = true;
                    }
                }
            } while (advanced);
        }

        private void CheckTriggerEvents(PlayerResourceState resources)
        {
            var activeQuests = _questRepository.GetActiveQuests();
            foreach (var entry in activeQuests)
            {
                var quest = _questDatabase.GetById(entry.QuestId);
                if (quest?.Stages == null || entry.CurrentStageIndex >= quest.Stages.Count) continue;

                if (IsBuildingHandover(quest, entry.CurrentStageIndex)) continue;

                var stage = quest.Stages[entry.CurrentStageIndex];
                var eventData = SelectFirstEligibleTriggerEvent(stage);
                if (eventData == null) continue;

                if (_eventTrigger.TriggerEvent(eventData)) return;
            }
        }

        internal EventData SelectFirstEligibleTriggerEvent(QuestStageData stage)
        {
            if (stage.TriggerEventIds == null || stage.TriggerEventIds.Length == 0) return null;

            foreach (string eventId in stage.TriggerEventIds)
            {
                if (string.IsNullOrEmpty(eventId)) continue;

                var eventData = _eventDatabase.GetById(eventId);
                if (eventData == null || eventData.Choices == null || eventData.Choices.Count == 0) continue;

                if (!_eventSelector.CheckConditions(eventData.Conditions)) continue;

                return eventData;
            }
            return null;
        }

        private void CheckFailConditions(PlayerResourceState resources)
        {
            var activeQuests = _questRepository.GetActiveQuests();
            for (int i = activeQuests.Count - 1; i >= 0; i--)
            {
                QuestStateEntry entry = activeQuests[i];
                QuestData quest = _questDatabase.GetById(entry.QuestId);
                if (quest?.FailConditions == null || quest.FailConditions.Count == 0) continue;

                foreach (QuestFailCondition condition in quest.FailConditions)
                {
                    if (EvaluateFailCondition(condition, entry, resources))
                    {
                        _rewardApplier.ApplyFailConsequences(quest);
                        _questRepository.FailQuest(entry.QuestId);
                        break;
                    }
                }
            }
        }

        internal bool IsBuildingHandover(QuestData quest, int stageIndex)
        {
            if (quest.GiverBuilding == BuildingType.Unknown) return false;
            if (string.IsNullOrEmpty(quest.StartCityId)) return false;
            if (stageIndex != quest.Stages.Count - 1) return false;
            var currentCity = _cityEntryService.CurrentCity;
            return currentCity != null && currentCity.Id == quest.StartCityId;
        }

        internal bool EvaluateStageCondition(QuestStageCondition condition, QuestStateEntry entry)
        {
            switch (condition.Type)
            {
                case QuestStageConditionType.HasItem:
                    return _inventoryRepository.HasPlayerItem(condition.Param, condition.Value);
                case QuestStageConditionType.InCity:
                    return _cityEntryService.CurrentCity != null &&
                           _cityEntryService.CurrentCity.Id == condition.Param;
                case QuestStageConditionType.QuestFlag:
                {
                    string questId = entry.QuestId;
                    string flagId = condition.Param;
                    int dotIndex = condition.Param.IndexOf('.');
                    if (dotIndex >= 0)
                    {
                        questId = condition.Param.Substring(0, dotIndex);
                        flagId = condition.Param.Substring(dotIndex + 1);
                    }
                    return _questRepository.GetFlag(questId, flagId) >= condition.Value;
                }
                case QuestStageConditionType.InAnyCity:
                    return _cityEntryService.CurrentCity != null;
                case QuestStageConditionType.OnRoute:
                    return _cityEntryService.CurrentCity == null;
                default:
                    return false;
            }
        }

        private bool EvaluateFailCondition(QuestFailCondition condition, QuestStateEntry entry,
            PlayerResourceState resources)
        {
            switch (condition.Type)
            {
                case QuestFailConditionType.NoItem:
                    return !_inventoryRepository.HasPlayerItem(condition.Param, 1);
                case QuestFailConditionType.DangerAbove:
                    return resources.AccumulatedDanger > condition.Value;
                case QuestFailConditionType.DaysSinceStart:
                    return (_dayTracker.CurrentDay - entry.StartDay) > condition.Value;
                default:
                    return false;
            }
        }
    }
}

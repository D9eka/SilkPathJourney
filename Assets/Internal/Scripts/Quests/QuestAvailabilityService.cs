using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.Quests.Generated;
using Internal.Scripts.UI.WorldLabel;
using R3;
using Zenject;

namespace Internal.Scripts.Quests
{
    public class QuestAvailabilityService : IQuestAvailabilityProvider, IInitializable, IDisposable
    {
        private readonly QuestRepository _repository;
        private readonly QuestDatabase _questDatabase;
        private readonly EventDatabase _eventDatabase;
        private readonly ReactiveProperty<bool> _hasAvailableQuest = new(false);
        private IDisposable _subscription;
        private EventSelector _eventSelector;

        public ReadOnlyReactiveProperty<bool> HasAvailableQuest => _hasAvailableQuest;

        public QuestAvailabilityService(
            QuestRepository repository,
            QuestDatabase questDatabase,
            EventDatabase eventDatabase)
        {
            _repository = repository;
            _questDatabase = questDatabase;
            _eventDatabase = eventDatabase;
        }

        [Inject]
        public void Construct(EventSelector eventSelector)
        {
            _eventSelector = eventSelector;
        }

        public void Initialize()
        {
            _subscription = _repository.Changed.Subscribe(_ => RecomputeHasAvailable());
            RecomputeHasAvailable();
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }

        public bool IsAvailable(string questId)
        {
            var quest = _questDatabase?.GetById(questId);
            return quest != null && IsAvailable(quest);
        }

        public bool IsAvailable(QuestData quest)
        {
            if (_repository.IsActive(quest.Id) || _repository.IsCompleted(quest.Id) || _repository.IsFailed(quest.Id))
                return false;

            if (quest.OrderInBranch <= 1)
                return CheckBriefingConditions(quest);

            var allQuests = _questDatabase?.Quests;
            if (allQuests == null) return false;

            int prevOrder = quest.OrderInBranch - 1;
            bool prevFound = false;
            foreach (var candidate in allQuests)
            {
                if (candidate.Branch == quest.Branch && candidate.OrderInBranch == prevOrder)
                {
                    if (!_repository.IsCompleted(candidate.Id))
                        return false;
                    prevFound = true;
                    break;
                }
            }

            if (!prevFound) return false;

            return CheckBriefingConditions(quest);
        }

        private bool CheckBriefingConditions(QuestData quest)
        {
            if (string.IsNullOrEmpty(quest.BriefingEventId)) return true;
            var briefing = _eventDatabase?.GetById(quest.BriefingEventId);
            if (briefing?.Conditions == null || briefing.Conditions.Count == 0) return true;
            return _eventSelector.CheckConditions(briefing.Conditions);
        }

        public QuestData GetAvailableForBuilding(BuildingType building, string cityId)
        {
            if (_questDatabase?.Quests == null) return null;

            foreach (var quest in _questDatabase.Quests)
            {
                if (quest.GiverBuilding != building) continue;
                if (quest.StartCityId != cityId) continue;
                if (!IsAvailable(quest)) continue;
                return quest;
            }

            return null;
        }

        public QuestData GetActiveStageInBuildingCity(string cityId, BuildingType building)
        {
            var activeQuests = _repository.GetActiveQuests();
            if (activeQuests == null) return null;

            foreach (var entry in activeQuests)
            {
                var questData = _questDatabase?.GetById(entry.QuestId);
                if (questData?.Stages == null) continue;

                int stageIndex = entry.CurrentStageIndex;
                if (stageIndex < 0 || stageIndex >= questData.Stages.Count) continue;

                var autoCondition = questData.Stages[stageIndex].AutoCompleteCondition;
                if (autoCondition.Type == QuestStageConditionType.InCity && autoCondition.Param == cityId)
                    return questData;
            }

            return null;
        }

        public QuestData GetBuildingHandover(BuildingType building, string cityId)
        {
            if (building == BuildingType.Unknown || string.IsNullOrEmpty(cityId)) return null;

            var activeQuests = _repository.GetActiveQuests();
            if (activeQuests == null) return null;

            foreach (var entry in activeQuests)
            {
                var questData = _questDatabase?.GetById(entry.QuestId);
                if (questData?.Stages == null || questData.Stages.Count == 0) continue;
                if (questData.GiverBuilding != building) continue;
                if (questData.StartCityId != cityId) continue;
                if (entry.CurrentStageIndex != questData.Stages.Count - 1) continue;
                return questData;
            }

            return null;
        }

        public bool HasActiveStageInCity(string cityId)
        {
            var activeQuests = _repository.GetActiveQuests();
            if (activeQuests == null) return false;

            foreach (var entry in activeQuests)
            {
                var questData = _questDatabase?.GetById(entry.QuestId);
                if (questData?.Stages == null) continue;

                int stageIndex = entry.CurrentStageIndex;
                if (stageIndex < 0 || stageIndex >= questData.Stages.Count) continue;

                var autoCondition = questData.Stages[stageIndex].AutoCompleteCondition;
                if (autoCondition.Type == QuestStageConditionType.InCity && autoCondition.Param == cityId)
                    return true;
            }

            return false;
        }

        public bool HasAvailableInCity(string cityId)
        {
            if (_questDatabase?.Quests == null) return false;

            foreach (var quest in _questDatabase.Quests)
            {
                if (quest.StartCityId != cityId) continue;
                if (IsAvailable(quest)) return true;
            }

            return false;
        }

        public IEnumerable<QuestData> EnumerateAvailableInCity(string cityId)
        {
            if (_questDatabase?.Quests == null) yield break;

            foreach (var quest in _questDatabase.Quests)
            {
                if (quest.StartCityId != cityId) continue;
                if (IsAvailable(quest)) yield return quest;
            }
        }

        private void RecomputeHasAvailable()
        {
            if (_questDatabase?.Quests == null)
            {
                _hasAvailableQuest.Value = false;
                return;
            }

            foreach (var quest in _questDatabase.Quests)
            {
                if (IsAvailable(quest))
                {
                    _hasAvailableQuest.Value = true;
                    return;
                }
            }

            _hasAvailableQuest.Value = false;
        }
    }
}

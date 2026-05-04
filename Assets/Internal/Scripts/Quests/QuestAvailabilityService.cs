using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Buildings;
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
        private readonly ReactiveProperty<bool> _hasAvailableQuest = new(false);
        private IDisposable _subscription;

        public ReadOnlyReactiveProperty<bool> HasAvailableQuest => _hasAvailableQuest;

        public QuestAvailabilityService(QuestRepository repository, QuestDatabase questDatabase)
        {
            _repository = repository;
            _questDatabase = questDatabase;
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
                return true;

            var allQuests = _questDatabase?.Quests;
            if (allQuests == null) return false;

            int prevOrder = quest.OrderInBranch - 1;
            foreach (var candidate in allQuests)
            {
                if (candidate.Branch == quest.Branch && candidate.OrderInBranch == prevOrder)
                    return _repository.IsCompleted(candidate.Id);
            }

            return false;
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

        public QuestData GetActiveStageInBuildingCity(string cityId)
        {
            var activeQuests = _repository.GetActiveQuests();
            if (activeQuests == null) return null;

            foreach (var entry in activeQuests)
            {
                var questData = _questDatabase?.GetById(entry.QuestId);
                if (questData?.Stages == null) continue;

                int stageIndex = entry.CurrentStageIndex;
                if (stageIndex < 0 || stageIndex >= questData.Stages.Count) continue;

                var condition = questData.Stages[stageIndex].AutoCompleteCondition;
                if (condition.Type == QuestStageConditionType.InCity && condition.Param == cityId)
                    return questData;
            }

            return null;
        }

        public bool HasActiveStageInCity(string cityId)
            => GetActiveStageInBuildingCity(cityId) != null;

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

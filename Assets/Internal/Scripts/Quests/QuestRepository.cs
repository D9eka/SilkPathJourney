using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events;
using Internal.Scripts.Player;
using Internal.Scripts.Quests.Save;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Save;
using Internal.Scripts.UI.WorldLabel;
using R3;
using Zenject;

namespace Internal.Scripts.Quests
{
    public class QuestRepository : IInitializable, IQuestAvailabilityProvider
    {
        private readonly SaveRepository _saveRepository;
        private readonly DayTracker _dayTracker;
        private readonly ICityEntryService _cityEntryService;
        private readonly IPlayerStateProvider _playerState;
        private readonly IRoadNodeLookup _nodeLookup;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly ReactiveProperty<int> _version = new(0);
        private readonly ReactiveProperty<bool> _hasAvailableQuest = new(false);

        public QuestRepository(
            SaveRepository saveRepository,
            DayTracker dayTracker,
            ICityEntryService cityEntryService,
            IPlayerStateProvider playerState,
            IRoadNodeLookup nodeLookup,
            ICityNodeResolver cityNodeResolver)
        {
            _saveRepository = saveRepository;
            _dayTracker = dayTracker;
            _cityEntryService = cityEntryService;
            _playerState = playerState;
            _nodeLookup = nodeLookup;
            _cityNodeResolver = cityNodeResolver;
        }

        public event Action<string> QuestStarted;
        public event Action<string> QuestAdvanced;
        public event Action<string> QuestCompleted;
        public event Action<string> QuestFailed;

        public Observable<int> Changed => _version;
        public ReadOnlyReactiveProperty<bool> HasAvailableQuest => _hasAvailableQuest;

        private QuestSaveData QuestData => _saveRepository.Data.Quests;

        public void Initialize()
        {
            _ = _saveRepository.Data.Quests;
        }

        public void StartQuest(string questId)
        {
            if (IsActive(questId) || IsCompleted(questId)) return;

            QuestData.ActiveQuests.Add(new QuestStateEntry
            {
                QuestId = questId,
                CurrentStageIndex = 0,
                StartDay = _dayTracker.CurrentDay,
                StartCityId = ResolveCurrentCityId()
            });

            if (string.IsNullOrEmpty(QuestData.TrackedQuestId))
                QuestData.TrackedQuestId = questId;

            SaveAndNotify();
            QuestStarted?.Invoke(questId);
        }

        public void AdvanceQuest(string questId)
        {
            var entry = GetActiveEntry(questId);
            if (entry == null) return;

            entry.CurrentStageIndex++;
            SaveAndNotify();
            QuestAdvanced?.Invoke(questId);
        }

        public void CompleteQuest(string questId)
        {
            if (!IsActive(questId)) return;

            QuestData.ActiveQuests.RemoveAll(e => e.QuestId == questId);
            if (!QuestData.CompletedQuestIds.Contains(questId))
                QuestData.CompletedQuestIds.Add(questId);
            ClearTrackedIfMatches(questId);
            SaveAndNotify();
            QuestCompleted?.Invoke(questId);
        }

        public void FailQuest(string questId)
        {
            if (!IsActive(questId)) return;

            QuestData.ActiveQuests.RemoveAll(e => e.QuestId == questId);
            if (!QuestData.FailedQuestIds.Contains(questId))
                QuestData.FailedQuestIds.Add(questId);
            ClearTrackedIfMatches(questId);
            SaveAndNotify();
            QuestFailed?.Invoke(questId);
        }

        public bool IsActive(string questId)
        {
            return QuestData.ActiveQuests.Any(e => e.QuestId == questId);
        }

        public bool IsCompleted(string questId)
        {
            return QuestData.CompletedQuestIds.Contains(questId);
        }

        public bool IsFailed(string questId)
        {
            return QuestData.FailedQuestIds.Contains(questId);
        }

        public int GetCurrentStageIndex(string questId)
        {
            var entry = GetActiveEntry(questId);
            return entry?.CurrentStageIndex ?? -1;
        }

        public QuestStateEntry GetActiveEntry(string questId)
        {
            return QuestData.ActiveQuests.FirstOrDefault(e => e.QuestId == questId);
        }

        public IReadOnlyList<QuestStateEntry> GetActiveQuests()
        {
            return QuestData.ActiveQuests;
        }

        public IReadOnlyList<string> GetCompletedQuestIds()
        {
            return QuestData.CompletedQuestIds;
        }

        public IReadOnlyList<string> GetFailedQuestIds()
        {
            return QuestData.FailedQuestIds;
        }

        public string GetTrackedQuestId()
        {
            return QuestData.TrackedQuestId;
        }

        public void TrackQuest(string questId)
        {
            if (!string.IsNullOrEmpty(questId) && !IsActive(questId)) return;
            QuestData.TrackedQuestId = questId;
            SaveAndNotify();
        }

        public int GetFlag(string questId, string flagId)
        {
            var entry = GetActiveEntry(questId);
            if (entry == null) return 0;

            var flag = entry.Flags.FirstOrDefault(f => f.FlagId == flagId);
            return flag?.Value ?? 0;
        }

        public void SetFlag(string questId, string flagId, int value)
        {
            var entry = GetActiveEntry(questId);
            if (entry == null) return;

            var flag = entry.Flags.FirstOrDefault(f => f.FlagId == flagId);
            if (flag != null)
            {
                flag.Value = value;
            }
            else
            {
                entry.Flags.Add(new QuestFlagEntry { FlagId = flagId, Value = value });
            }

            SaveAndNotify();
        }

        public void IncrementFlag(string questId, string flagId, int delta)
        {
            SetFlag(questId, flagId, GetFlag(questId, flagId) + delta);
        }

        private void ClearTrackedIfMatches(string questId)
        {
            if (QuestData.TrackedQuestId == questId)
                QuestData.TrackedQuestId = null;
        }

        private string ResolveCurrentCityId()
        {
            var current = _cityEntryService.CurrentCity;
            if (current != null) return current.Id;

            string nearestNode = _nodeLookup.FindNearestNodeId(_playerState.CurrentPosition);
            if (!string.IsNullOrEmpty(nearestNode) && _cityNodeResolver.TryGetCityByNodeId(nearestNode, out var nearCity))
                return nearCity.Id;

            return string.Empty;
        }

        private void SaveAndNotify()
        {
            _saveRepository.Save();
            _version.Value++;
        }
    }
}

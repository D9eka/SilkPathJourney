using System;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Quests;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Quests;
using Internal.Scripts.UI.StackService;
using R3;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Building
{
    public readonly struct BuildingQuestSlotState
    {
        public string Description { get; }
        public string EndingBranchId { get; }

        public bool IsEndingOffer => EndingBranchId != null;

        public BuildingQuestSlotState(string description) { Description = description; EndingBranchId = null; }
        public BuildingQuestSlotState(string description, string endingBranchId) { Description = description; EndingBranchId = endingBranchId; }
    }

    public class BuildingQuestSlotViewModel : IDisposable
    {
        private enum SlotKind { None, PendingEnding, AvailableQuest, ActiveStage }

        private readonly struct SlotResolution
        {
            public readonly SlotKind Kind;
            public readonly QuestData Quest;
            public readonly string PendingBranchId;

            public SlotResolution(SlotKind kind, QuestData quest, string pendingBranchId)
            {
                Kind = kind;
                Quest = quest;
                PendingBranchId = pendingBranchId;
            }
        }

        private readonly QuestRepository _questRepository;
        private readonly QuestAvailabilityService _availability;
        private readonly QuestPendingEndingsService _pendingEndings;
        private readonly EventDatabase _eventDb;
        private readonly EventTrigger _eventTrigger;
        private readonly ScreenStackService _screenStackService;

        private readonly ReactiveProperty<BuildingQuestSlotState?> _state = new();

        private BuildingType _building;
        private string _cityId;
        private QuestData _resolvedQuest;
        private string _pendingEndingBranchId;

        public Observable<BuildingQuestSlotState?> State => _state;

        public BuildingQuestSlotViewModel(
            QuestRepository questRepository,
            QuestAvailabilityService availability,
            QuestPendingEndingsService pendingEndings,
            EventDatabase eventDb,
            EventTrigger eventTrigger,
            ScreenStackService screenStackService)
        {
            _questRepository = questRepository;
            _availability = availability;
            _pendingEndings = pendingEndings;
            _eventDb = eventDb;
            _eventTrigger = eventTrigger;
            _screenStackService = screenStackService;
        }

        public void Bind(BuildingType building, string cityId)
        {
            Dispose();

            _building = building;
            _cityId = cityId;

            _questRepository.QuestStarted += OnQuestChanged;
            _questRepository.QuestAdvanced += OnQuestChanged;
            _questRepository.QuestCompleted += OnQuestChanged;
            _questRepository.QuestFailed += OnQuestChanged;
            _pendingEndings.PendingEndingsChanged += OnPendingEndingsChanged;

            Refresh();
        }

        public void Dispose()
        {
            _questRepository.QuestStarted -= OnQuestChanged;
            _questRepository.QuestAdvanced -= OnQuestChanged;
            _questRepository.QuestCompleted -= OnQuestChanged;
            _questRepository.QuestFailed -= OnQuestChanged;
            _pendingEndings.PendingEndingsChanged -= OnPendingEndingsChanged;

            _resolvedQuest = null;
            _pendingEndingBranchId = null;
            _state.Value = null;
        }

        public void OnTalk()
        {
            if (TryTriggerPendingEnding()) return;
            if (TryTriggerAvailableBriefing()) return;
            if (TryTriggerActiveStage()) return;
            _screenStackService.TryOpen(ScreenId.Quests, out _);
        }

        private bool TryTriggerPendingEnding()
        {
            if (_pendingEndingBranchId == null) return false;
            string eventId = QuestPendingEndingsService.GetFinaleOfferEventId(_pendingEndingBranchId);
            var eventData = _eventDb.GetById(eventId);
            if (eventData == null) return false;
            _eventTrigger.TriggerEvent(eventData);
            return true;
        }

        private bool TryTriggerAvailableBriefing()
        {
            if (_resolvedQuest == null || _pendingEndingBranchId != null) return false;
            var available = _availability.GetAvailableForBuilding(_building, _cityId);
            if (available == null) return false;
            if (string.IsNullOrEmpty(available.BriefingEventId)) return false;
            var eventData = _eventDb.GetById(available.BriefingEventId);
            if (eventData == null) return false;
            _eventTrigger.TriggerEvent(eventData);
            return true;
        }

        private bool TryTriggerActiveStage()
        {
            if (_resolvedQuest == null || _pendingEndingBranchId != null) return false;
            if (_availability.GetAvailableForBuilding(_building, _cityId) != null) return false;
            int stageIndex = _questRepository.GetCurrentStageIndex(_resolvedQuest.Id);
            if (stageIndex < 0 || _resolvedQuest.Stages == null || stageIndex >= _resolvedQuest.Stages.Count) return false;
            string[] triggerEventIds = _resolvedQuest.Stages[stageIndex].TriggerEventIds;
            if (triggerEventIds == null || triggerEventIds.Length == 0) return false;
            foreach (string triggerEventId in triggerEventIds)
            {
                if (string.IsNullOrEmpty(triggerEventId)) continue;
                var eventData = _eventDb.GetById(triggerEventId);
                if (eventData == null) continue;
                if (_eventTrigger.TriggerEvent(eventData)) return true;
            }
            return false;
        }

        private void OnQuestChanged(string _) => Refresh();
        private void OnPendingEndingsChanged() => Refresh();

        private void Refresh() => Apply(Resolve());

        private SlotResolution Resolve()
        {
            foreach (var branchId in _pendingEndings.GetPendingEndingsForBuilding(_building, _cityId))
                return new SlotResolution(SlotKind.PendingEnding, null, branchId);

            var available = _availability.GetAvailableForBuilding(_building, _cityId);
            if (available != null)
                return new SlotResolution(SlotKind.AvailableQuest, available, null);

            var active = _availability.GetActiveStageInBuildingCity(_cityId);
            if (active != null)
                return new SlotResolution(SlotKind.ActiveStage, active, null);

            return new SlotResolution(SlotKind.None, null, null);
        }

        private void Apply(SlotResolution r)
        {
            _pendingEndingBranchId = r.PendingBranchId;
            _resolvedQuest = r.Quest;

            switch (r.Kind)
            {
                case SlotKind.PendingEnding:
                    string endingDesc = LocalizationService.ResolveString(
                        new LocalizedString("UI", "UI.Building.QuestSlot.EndingOffer"),
                        "Завершить путешествие",
                        "BuildingQuestSlot.EndingOffer");
                    _state.Value = new BuildingQuestSlotState(endingDesc, r.PendingBranchId);
                    break;

                case SlotKind.AvailableQuest:
                    string questDesc = LocalizationService.ResolveString(r.Quest.Description, r.Quest.Id, QuestLocContext.QuestDesc(r.Quest.Id));
                    _state.Value = new BuildingQuestSlotState(questDesc);
                    break;

                case SlotKind.ActiveStage:
                    _state.Value = new BuildingQuestSlotState(GetCurrentStageDescription(r.Quest));
                    break;

                default:
                    _state.Value = null;
                    break;
            }
        }

        private string GetCurrentStageDescription(QuestData quest)
        {
            int stageIndex = _questRepository.GetCurrentStageIndex(quest.Id);
            if (stageIndex >= 0 && quest.Stages != null && stageIndex < quest.Stages.Count)
            {
                var stage = quest.Stages[stageIndex];
                return LocalizationService.ResolveString(stage.Description, stage.Id, QuestLocContext.StageDesc(quest.Id, stage.Id));
            }

            return LocalizationService.ResolveString(quest.Description, quest.Id, QuestLocContext.QuestDesc(quest.Id));
        }
    }
}

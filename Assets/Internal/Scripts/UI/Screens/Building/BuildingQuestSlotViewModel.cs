using System;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Quests;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Quests;
using R3;

namespace Internal.Scripts.UI.Screens.Building
{
    public readonly struct BuildingQuestSlotState
    {
        public string Description { get; }
        public BuildingQuestSlotState(string description) { Description = description; }
    }

    public class BuildingQuestSlotViewModel : IDisposable
    {
        private readonly QuestRepository _questRepository;
        private readonly EventDatabase _eventDb;
        private readonly EventTrigger _eventTrigger;
        private readonly ScreenStackService _screenStackService;

        private readonly ReactiveProperty<BuildingQuestSlotState?> _state = new();

        private BuildingType _building;
        private string _cityId;
        private QuestData _resolvedQuest;
        private bool _isAvailable;

        public Observable<BuildingQuestSlotState?> State => _state;

        public BuildingQuestSlotViewModel(
            QuestRepository questRepository,
            EventDatabase eventDb,
            EventTrigger eventTrigger,
            ScreenStackService screenStackService)
        {
            _questRepository = questRepository;
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

            Refresh();
        }

        public void Dispose()
        {
            _questRepository.QuestStarted -= OnQuestChanged;
            _questRepository.QuestAdvanced -= OnQuestChanged;
            _questRepository.QuestCompleted -= OnQuestChanged;
            _questRepository.QuestFailed -= OnQuestChanged;

            _resolvedQuest = null;
            _state.Value = null;
        }

        public void OnTalk()
        {
            if (_resolvedQuest == null)
                return;

            if (_isAvailable)
            {
                if (!string.IsNullOrEmpty(_resolvedQuest.BriefingEventId))
                {
                    var eventData = _eventDb.GetById(_resolvedQuest.BriefingEventId);
                    if (eventData != null)
                    {
                        _eventTrigger.TriggerEvent(eventData);
                        return;
                    }
                }

                _screenStackService.TryOpen(ScreenId.Quests, out _);
                return;
            }

            int stageIndex = _questRepository.GetCurrentStageIndex(_resolvedQuest.Id);
            if (stageIndex >= 0 && _resolvedQuest.Stages != null && stageIndex < _resolvedQuest.Stages.Count)
            {
                string triggerEventId = _resolvedQuest.Stages[stageIndex].TriggerEventId;
                if (!string.IsNullOrEmpty(triggerEventId))
                {
                    var eventData = _eventDb.GetById(triggerEventId);
                    if (eventData != null)
                    {
                        _eventTrigger.TriggerEvent(eventData);
                        return;
                    }
                }
            }

            _screenStackService.TryOpen(ScreenId.Quests, out _);
        }

        private void OnQuestChanged(string _) => Refresh();

        private void Refresh()
        {
            var available = _questRepository.GetAvailableForBuilding(_building, _cityId);
            if (available != null)
            {
                _resolvedQuest = available;
                _isAvailable = true;
                string desc = LocalizationService.ResolveString(available.Description, available.Id, QuestLocContext.QuestDesc(available.Id));
                _state.Value = new BuildingQuestSlotState(desc);
                return;
            }

            var active = _questRepository.GetActiveStageInBuildingCity(_cityId);
            if (active != null)
            {
                _resolvedQuest = active;
                _isAvailable = false;
                string desc = GetCurrentStageDescription(active);
                _state.Value = new BuildingQuestSlotState(desc);
                return;
            }

            _resolvedQuest = null;
            _state.Value = null;
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

using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Quests;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.UI.Theme;
using R3;

namespace Internal.Scripts.UI.Screens.Quests
{
    public sealed class QuestsScreenViewModel : ScreenViewModelBase
    {
        private readonly QuestRepository _questRepository;
        private readonly QuestDatabase _questDatabase;
        private readonly EconomyDatabase _economyDatabase;
        private readonly ScreenStackService _screenStackService;
        private readonly UiThemeService _themeService;

        private readonly ReactiveProperty<QuestsViewState> _state = new(QuestsViewState.Empty);
        private IDisposable _changedSubscription;
        private string _selectedQuestId;

        public QuestsScreenViewModel(
            QuestRepository questRepository,
            QuestDatabase questDatabase,
            EconomyDatabase economyDatabase,
            ScreenStackService screenStackService,
            UiThemeService themeService)
        {
            _questRepository = questRepository;
            _questDatabase = questDatabase;
            _economyDatabase = economyDatabase;
            _screenStackService = screenStackService;
            _themeService = themeService;
        }

        public string GetRuntimeStartCityId(string questId)
        {
            return _questRepository.GetActiveEntry(questId)?.StartCityId;
        }

        public UiThemeService ThemeService => _themeService;
        public EconomyDatabase EconomyDatabase => _economyDatabase;

        public override ScreenId Id => ScreenId.Quests;
        public Observable<QuestsViewState> State => _state;

        public QuestData GetQuestData(string questId)
        {
            return _questDatabase.GetById(questId);
        }

        public void SelectQuest(string questId)
        {
            _selectedQuestId = questId;
            RebuildState();
        }

        public void TrackQuest(string questId)
        {
            _questRepository.TrackQuest(questId);
        }

        public void DeclineQuest(string questId)
        {
            _questRepository.FailQuest(questId);
        }

        protected override void OnOpen(object args)
        {
            _changedSubscription = _questRepository.Changed.Subscribe(_ => RebuildState());
            RebuildState();
        }

        protected override void OnClose()
        {
            _changedSubscription?.Dispose();
            _changedSubscription = null;
        }

        private void RebuildState()
        {
            var activeEntries = _questRepository.GetActiveQuests();
            var quests = new List<QuestViewData>(activeEntries.Count);
            string trackedId = _questRepository.GetTrackedQuestId();

            bool selectedFound = false;

            for (int i = 0; i < activeEntries.Count; i++)
            {
                var entry = activeEntries[i];
                var data = _questDatabase.GetById(entry.QuestId);
                if (data == null) continue;

                bool isSelected = entry.QuestId == _selectedQuestId;
                if (isSelected) selectedFound = true;

                quests.Add(BuildViewData(data, entry.QuestId, isSelected,
                    currentStageIndex: entry.CurrentStageIndex,
                    isTracked: entry.QuestId == trackedId));
            }

            foreach (string completedId in _questRepository.GetCompletedQuestIds())
            {
                var data = _questDatabase.GetById(completedId);
                if (data == null) continue;

                bool isSelected = completedId == _selectedQuestId;
                if (isSelected) selectedFound = true;

                quests.Add(BuildViewData(data, completedId, isSelected,
                    currentStageIndex: data.Stages?.Count ?? 0,
                    isCompleted: true));
            }

            foreach (string failedId in _questRepository.GetFailedQuestIds())
            {
                var data = _questDatabase.GetById(failedId);
                if (data == null) continue;

                bool isSelected = failedId == _selectedQuestId;
                if (isSelected) selectedFound = true;

                quests.Add(BuildViewData(data, failedId, isSelected, isFailed: true));
            }

            if (!selectedFound && quests.Count > 0)
            {
                _selectedQuestId = quests[0].QuestId;
                var first = quests[0];
                first.IsSelected = true;
                quests[0] = first;
            }

            _state.Value = new QuestsViewState
            {
                Quests = quests,
                SelectedQuestId = _selectedQuestId
            };
        }

        private QuestViewData BuildViewData(
            QuestData data,
            string questId,
            bool isSelected,
            int currentStageIndex = 0,
            bool isTracked = false,
            bool isCompleted = false,
            bool isFailed = false)
        {
            return new QuestViewData
            {
                QuestId = questId,
                BranchName = LocalizationService.ResolveString(
                    data.BranchName, data.Branch.ToString(),
                    QuestLocContext.BranchName(data.Branch)),
                OrderInBranch = data.OrderInBranch,
                TotalStages = data.Stages?.Count ?? 0,
                CurrentStageIndex = currentStageIndex,
                IsSelected = isSelected,
                IsTracked = isTracked,
                IsCompleted = isCompleted,
                IsFailed = isFailed
            };
        }
    }
}

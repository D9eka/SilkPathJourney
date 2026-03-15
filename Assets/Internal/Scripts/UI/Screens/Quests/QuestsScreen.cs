using System;
using System.Collections.Generic;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Quests
{
    public class QuestsScreen : PopupScreen
    {
        [Header("Content")]
        [SerializeField] private GameObject _emptyStatePlaceholder;
        [SerializeField] private TextMeshProUGUI _emptyStateText;
        [SerializeField] private LocalizedString _emptyStateLocalizedString;
        [SerializeField] private Transform _questListContent;
        [SerializeField] private QuestListItemView _questListItemPrefab;
        [SerializeField] private QuestDetailView _detailView;

        private QuestsScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private LocalizationService.LocalizedTextHandle _emptyStateHandle;
        private readonly List<QuestListItemView> _spawnedItems = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            BindEmptyStateLocalization();
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            UnsubscribeViewModel();
            _emptyStateHandle?.Dispose();
            _emptyStateHandle = null;
            base.OnDisable();
        }

        protected override void OnLocalizationReady()
        {
            base.OnLocalizationReady();
            BindEmptyStateLocalization();
        }

        private void BindEmptyStateLocalization()
        {
            _emptyStateHandle?.Dispose();
            if (_emptyStateText != null && _emptyStateLocalizedString != null && Localization != null)
                _emptyStateHandle = Localization.BindText(_emptyStateText, _emptyStateLocalizedString, $"{name}.EmptyState");
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as QuestsScreenViewModel;
            SubscribeViewModel();
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
                return;

            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
        }

        private void UnsubscribeViewModel()
        {
            if (_viewModel == null)
                return;

            _stateSubscription?.Dispose();
            _stateSubscription = null;
        }

        private void ApplyState(QuestsViewState state)
        {
            RebuildList(state);
            UpdateDetail(state);
        }

        private void RebuildList(QuestsViewState state)
        {
            int count = state.Quests?.Count ?? 0;
            _emptyStatePlaceholder.SetActive(count == 0);
            
            EnsurePool(count);

            for (int i = 0; i < count; i++)
            {
                var item = _spawnedItems[i];
                item.gameObject.SetActive(true);
                item.SetData(state.Quests[i], _viewModel);
            }

            for (int i = count; i < _spawnedItems.Count; i++)
                _spawnedItems[i].gameObject.SetActive(false);
        }

        private void UpdateDetail(QuestsViewState state)
        {
            if (_detailView == null) return;

            if (string.IsNullOrEmpty(state.SelectedQuestId))
            {
                _detailView.ShowEmpty();
                return;
            }

            QuestData quest = _viewModel.GetQuestData(state.SelectedQuestId);
            if (quest == null)
            {
                _detailView.ShowEmpty();
                return;
            }

            int stageIndex = 0;
            bool isFailed = false;
            bool isCompleted = false;
            if (state.Quests != null)
            {
                for (int i = 0; i < state.Quests.Count; i++)
                {
                    if (state.Quests[i].QuestId == state.SelectedQuestId)
                    {
                        stageIndex = state.Quests[i].CurrentStageIndex;
                        isFailed = state.Quests[i].IsFailed;
                        isCompleted = state.Quests[i].IsCompleted;
                        break;
                    }
                }
            }

            string runtimeStartCity = _viewModel.GetRuntimeStartCityId(state.SelectedQuestId);
            _detailView.ShowQuest(quest, stageIndex, _viewModel.EconomyDatabase, runtimeStartCity, isFailed, isCompleted);
        }

        private void EnsurePool(int count)
        {
            while (_spawnedItems.Count < count)
            {
                var view = Instantiate(_questListItemPrefab, _questListContent);
                view.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                view.SetThemeService(_viewModel?.ThemeService);
                _spawnedItems.Add(view);
            }
        }
    }
}

using System.Collections.Generic;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

using static Internal.Scripts.UI.Screens.Quests.QuestLocContext;

namespace Internal.Scripts.UI.Screens.Quests
{
    public class QuestListItemView : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _typeText;
        [SerializeField] private TextMeshProUGUI _rewardText;

        [Header("Stages")]
        [SerializeField] private Transform _stagesContainer;
        [SerializeField] private QuestStageCheckView _stagePrefab;

        [Header("Actions")]
        [SerializeField] private Button _trackButton;
        [SerializeField] private TextMeshProUGUI _trackButtonText;
        [SerializeField] private Button _refuseButton;
        [SerializeField] private TextMeshProUGUI _refuseButtonText;

        [Header("Selection")]
        [SerializeField] private GameObject _border;
        [SerializeField] private Button _selectButton;

        private readonly List<QuestStageCheckView> _spawnedStages = new();
        private QuestsScreenViewModel _viewModel;
        private string _questId;
        private UiThemeService _themeService;

        public void SetThemeService(UiThemeService themeService)
        {
            _themeService = themeService;
        }

        public void SetData(QuestViewData data, QuestsScreenViewModel viewModel)
        {
            _viewModel = viewModel;
            _questId = data.QuestId;

            QuestData quest = viewModel.GetQuestData(data.QuestId);

            if (_nameText != null && quest != null)
                _nameText.text = LocalizationService.ResolveString(
                    quest.Name, quest.Id, QuestName(quest.Id));

            if (_typeText != null)
                _typeText.text = data.BranchName;

            if (_border != null)
                _border.SetActive(data.IsTracked);

            RebuildStages(quest, data.CurrentStageIndex, data.TotalStages);
            SetRewardText(quest);
            BindButtons(data.IsTracked);
        }

        private void RebuildStages(QuestData quest, int currentStageIndex, int totalStages)
        {
            if (_stagesContainer == null || _stagePrefab == null || quest == null)
                return;

            EnsureStagePool(totalStages);

            for (int i = 0; i < totalStages; i++)
            {
                var view = _spawnedStages[i];
                view.gameObject.SetActive(true);

                StageDisplayState state;
                if (i < currentStageIndex)
                    state = StageDisplayState.Completed;
                else if (i == currentStageIndex)
                    state = StageDisplayState.Current;
                else
                    state = StageDisplayState.Future;

                string desc = i < quest.Stages.Count
                    ? LocalizationService.ResolveString(
                        quest.Stages[i].Description,
                        quest.Stages[i].Id,
                        StageDesc(quest.Id, quest.Stages[i].Id))
                    : string.Empty;

                view.SetState(state, desc);
            }

            for (int i = totalStages; i < _spawnedStages.Count; i++)
                _spawnedStages[i].gameObject.SetActive(false);
        }

        private void SetRewardText(QuestData quest)
        {
            if (_rewardText == null || quest == null) return;
            _rewardText.text = QuestRewardFormatter.FormatRewards(quest.Rewards);
        }

        private void BindButtons(bool isTracked)
        {
            if (_selectButton != null)
            {
                _selectButton.onClick.RemoveAllListeners();
                _selectButton.onClick.AddListener(() => _viewModel?.SelectQuest(_questId));
            }

            if (_trackButton != null)
            {
                _trackButton.onClick.RemoveAllListeners();
                string toggleId = isTracked ? null : _questId;
                _trackButton.onClick.AddListener(() => _viewModel?.TrackQuest(toggleId));
            }

            if (_trackButtonText != null)
                _trackButtonText.text = isTracked
                    ? LocalizationService.ResolveString(new LocalizedString("UI", "UI.Quest.Untrack"), "Untrack", "QuestList")
                    : LocalizationService.ResolveString(new LocalizedString("UI", "UI.Quest.Track"), "Track", "QuestList");

            if (_refuseButton != null)
            {
                _refuseButton.onClick.RemoveAllListeners();
                _refuseButton.onClick.AddListener(() => _viewModel?.DeclineQuest(_questId));
            }

            if (_refuseButtonText != null)
                _refuseButtonText.text = LocalizationService.ResolveString(new LocalizedString("UI", "UI.Quest.Decline"), "Decline", "QuestList");
        }

        private void EnsureStagePool(int count)
        {
            while (_spawnedStages.Count < count)
            {
                var view = Instantiate(_stagePrefab, _stagesContainer);
                view.gameObject.InitializeColorBinders(themeService: _themeService);
                _spawnedStages.Add(view);
            }
        }
    }
}

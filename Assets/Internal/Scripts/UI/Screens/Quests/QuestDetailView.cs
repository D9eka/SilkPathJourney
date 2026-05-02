using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.Quests.Generated;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static Internal.Scripts.UI.Screens.Quests.QuestLocContext;

namespace Internal.Scripts.UI.Screens.Quests
{
    public class QuestDetailView : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _currentObjectiveText;
        [SerializeField] private Image _questImage;
        [SerializeField] private TextMeshProUGUI _citiesText;

        [Header("Branch")]
        [SerializeField] private TextMeshProUGUI _branchNameText;
        [SerializeField] private TextMeshProUGUI _branchDescriptionText;

        [Header("Lists")]
        [SerializeField] private QuestDataListView _rewardsList;
        [SerializeField] private QuestDataListView _failList;

        private EconomyDatabase _economyDatabase;
        private string _runtimeStartCityId;

        public void ShowQuest(QuestData quest, int currentStageIndex, EconomyDatabase economyDatabase,
            string runtimeStartCityId = null, bool isFailed = false, bool isCompleted = false)
        {
            _economyDatabase = economyDatabase;
            _runtimeStartCityId = runtimeStartCityId;
            gameObject.SetActive(true);

            if (_titleText != null)
                _titleText.text = LocalizationService.ResolveString(
                    quest.Name, quest.Id, QuestName(quest.Id));

            if (_descriptionText != null)
                _descriptionText.text = LocalizationService.ResolveString(
                    quest.Description, string.Empty, QuestDesc(quest.Id));

            if (_currentObjectiveText != null)
            {
                if (isFailed)
                    _currentObjectiveText.text = $"<b>{LocalizationService.Resolve(LocUI.Table, LocUI.UI_Quest_Failed)}</b>";
                else if (isCompleted)
                    _currentObjectiveText.text = $"<b>{LocalizationService.Resolve(LocUI.Table, LocUI.UI_Quest_Completed)}</b>";
                else if (quest.Stages != null && currentStageIndex < quest.Stages.Count)
                {
                    var stage = quest.Stages[currentStageIndex];
                    string desc = LocalizationService.ResolveString(
                        stage.Description, stage.Id, StageDesc(quest.Id, stage.Id));
                    string objectiveLabel = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Quest_Detail_CurrentObjective);
                    _currentObjectiveText.text = $"<b>{objectiveLabel} {desc}</b>";
                }
            }

            if (_questImage != null)
                _questImage.sprite = quest.Icon;

            if (_citiesText != null)
                _citiesText.text = BuildCitiesText(quest);

            if (_branchNameText != null)
                _branchNameText.text = LocalizationService.ResolveString(
                    quest.BranchName, quest.Branch.ToString(), BranchName(quest.Branch));

            if (_branchDescriptionText != null)
                _branchDescriptionText.text = LocalizationService.ResolveString(
                    quest.BranchDescription, string.Empty, BranchDesc(quest.Branch));

            RebuildRewards(quest);
            RebuildFailConditions(quest);
        }

        public void ShowEmpty()
        {
            gameObject.SetActive(false);
            _rewardsList?.Clear();
            _failList?.Clear();
        }

        private void RebuildRewards(QuestData quest)
        {
            if (_rewardsList == null) return;

            var rewards = quest.Rewards;
            if (rewards == null || rewards.Count == 0)
            {
                _rewardsList.SetItems(null);
                return;
            }

            var items = new List<string>(rewards.Count);
            for (int i = 0; i < rewards.Count; i++)
                items.Add(QuestRewardFormatter.FormatReward(rewards[i]));
            _rewardsList.SetHeader(LocalizationService.Resolve(LocUI.Table, LocUI.UI_Quest_Detail_RewardHeader));
            _rewardsList.SetItems(items);
        }

        private void RebuildFailConditions(QuestData quest)
        {
            if (_failList == null) return;

            var conditions = quest.FailConditions;
            if (conditions == null || conditions.Count == 0)
            {
                _failList.SetItems(null);
                return;
            }

            var items = new List<string>(conditions.Count);
            for (int i = 0; i < conditions.Count; i++)
                items.Add(FormatFailCondition(conditions[i]));
            _failList.SetHeader(LocalizationService.Resolve(LocUI.Table, LocUI.UI_Quest_Detail_FailHeader));
            _failList.SetItems(items);
        }

        private string FormatFailCondition(QuestFailCondition c)
        {
            switch (c.Type)
            {
                case QuestFailConditionType.DaysSinceStart:
                    return LocalizationService.Resolve(LocUI.Table, LocUI.UI_Quest_Fail_DaysSinceStart).Replace("{value}", c.Value.ToString());
                case QuestFailConditionType.DangerAbove:
                    return LocalizationService.Resolve(LocUI.Table, LocUI.UI_Quest_Fail_DangerAbove).Replace("{value}", c.Value.ToString());
                case QuestFailConditionType.NoItem:
                    return LocalizationService.Resolve(LocUI.Table, LocUI.UI_Quest_Fail_NoItem).Replace("{param}", c.Param);
                default:
                    return c.Type.ToString();
            }
        }

        private string BuildCitiesText(QuestData quest)
        {
            if (quest.Stages == null || quest.Stages.Count == 0)
                return string.Empty;

            var cityIds = new List<string>();

            string startCity = !string.IsNullOrEmpty(quest.StartCityId) ? quest.StartCityId : _runtimeStartCityId;
            if (!string.IsNullOrEmpty(startCity))
                cityIds.Add(startCity);

            for (int i = 0; i < quest.Stages.Count; i++)
            {
                var condition = quest.Stages[i].AutoCompleteCondition;
                if (condition.Type != QuestStageConditionType.InCity)
                    continue;

                string cityId = condition.Param;
                if (string.IsNullOrEmpty(cityId))
                    continue;

                if (cityIds.Count == 0 || !string.Equals(cityIds[cityIds.Count - 1], cityId, StringComparison.OrdinalIgnoreCase))
                    cityIds.Add(cityId);
            }

            if (cityIds.Count == 0)
                return string.Empty;

            var names = new List<string>(cityIds.Count);
            foreach (var cityId in cityIds)
                names.Add(ResolveCityName(cityId));

            return string.Join(" → ", names);
        }

        private string ResolveCityName(string cityId)
        {
            if (_economyDatabase != null)
            {
                var city = _economyDatabase.Cities.Find(c => string.Equals(c.Id, cityId, StringComparison.OrdinalIgnoreCase));
                if (city != null)
                    return LocalizationService.ResolveString(city.Name, cityId, "CityName." + cityId);
            }

            return char.ToUpper(cityId[0]) + cityId.Substring(1);
        }
    }
}

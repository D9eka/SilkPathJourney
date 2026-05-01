using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Quests.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Quests.Data
{
    [CreateAssetMenu(menuName = "SPJ/Quests/Quest Data", fileName = "QuestData")]
    public class QuestData : ScriptableObject
    {
        [Header("Quest Info")]
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; }
        [field: SerializeField] public LocalizedString Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }

        [field: SerializeField] public string StartCityId { get; private set; }

        [Header("Branch")]
        [field: SerializeField] public QuestBranch Branch { get; private set; }
        [field: SerializeField] public int OrderInBranch { get; private set; }
        [field: SerializeField] public LocalizedString BranchName { get; private set; }
        [field: SerializeField] public LocalizedString BranchDescription { get; private set; }

        [Header("Stages")]
        [field: SerializeField] public List<QuestStageData> Stages { get; private set; }

        [Header("Rewards")]
        [field: SerializeField] public List<QuestRewardData> Rewards { get; private set; }

        [Header("Fail Conditions")]
        [field: SerializeField] public List<QuestFailCondition> FailConditions { get; private set; }
        [field: SerializeField] public List<EventOutcomeEntry> FailConsequences { get; private set; }

        [Header("Building issuing")]
        [field: SerializeField] public BuildingType GiverBuilding { get; private set; }
        [field: SerializeField] public string BriefingEventId { get; private set; }

#if UNITY_EDITOR
        public void ApplyImport(
            string id,
            LocalizedString name,
            LocalizedString description,
            Sprite icon,
            QuestBranch branch,
            int orderInBranch,
            string startCityId,
            LocalizedString branchName,
            LocalizedString branchDescription,
            List<QuestStageData> stages,
            List<QuestRewardData> rewards,
            List<QuestFailCondition> failConditions,
            List<EventOutcomeEntry> failConsequences,
            BuildingType giverBuilding,
            string briefingEventId)
        {
            Id = id;
            Name = name;
            Description = description;
            Icon = icon;
            StartCityId = startCityId;
            Branch = branch;
            OrderInBranch = orderInBranch;
            BranchName = branchName;
            BranchDescription = branchDescription;
            Stages = stages ?? new List<QuestStageData>();
            Rewards = rewards ?? new List<QuestRewardData>();
            FailConditions = failConditions ?? new List<QuestFailCondition>();
            FailConsequences = failConsequences ?? new List<EventOutcomeEntry>();
            GiverBuilding = giverBuilding;
            BriefingEventId = briefingEventId ?? string.Empty;
        }
#endif
    }

    [Serializable]
    public struct QuestStageData
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public LocalizedString Description { get; private set; }
        [field: SerializeField] public string TriggerEventId { get; private set; }
        [field: SerializeField] public QuestStageCondition AutoCompleteCondition { get; private set; }

#if UNITY_EDITOR
        public QuestStageData(string id, LocalizedString description, string triggerEventId, QuestStageCondition autoCompleteCondition)
        {
            Id = id;
            Description = description;
            TriggerEventId = triggerEventId;
            AutoCompleteCondition = autoCompleteCondition;
        }
#endif
    }

    [Serializable]
    public struct QuestRewardData
    {
        [field: SerializeField] public QuestRewardType Type { get; private set; }
        [field: SerializeField] public string Target { get; private set; }
        [field: SerializeField] public int Value { get; private set; }

#if UNITY_EDITOR
        public QuestRewardData(QuestRewardType type, string target, int value)
        {
            Type = type;
            Target = target;
            Value = value;
        }
#endif
    }
}

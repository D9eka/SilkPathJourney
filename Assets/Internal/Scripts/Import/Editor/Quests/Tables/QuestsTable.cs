using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.Quests.Generated;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Quests.Tables
{
    public static class QuestsTable
    {
        private const string QUESTS_FOLDER = GENERATED_DATA_FOLDER + "/Quests";
        private const string SPRITES_FOLDER = "Assets/Internal/Sprites/UI/Window/Quest";

        public static List<QuestData> Import(
            Dictionary<string, QuestBranch> branchMap,
            Dictionary<QuestBranch, (string nameKey, string descKey)> branchLocMap,
            Dictionary<string, List<QuestStagesTable.StageRaw>> stages,
            Dictionary<string, List<QuestRewardsTable.RewardRaw>> rewards,
            Dictionary<string, List<QuestFailCondition>> failConditions,
            Dictionary<string, List<EventOutcomeEntry>> failConsequences,
            string locTableName,
            string csvFile = "quests.csv")
        {
            string csvPath = CsvPath(csvFile);
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            List<QuestData> quests = new();

            if (rows.Count == 0) return quests;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "quest_id");
            int nameKeyIndex = FindColumnIndex(header, "name_key");
            int descKeyIndex = FindColumnIndex(header, "description_key");
            int iconIndex = FindColumnIndex(header, "icon_name");
            int branchIdIndex = FindColumnIndex(header, "branch_id");
            int orderIndex = FindColumnIndex(header, "order_in_branch");
            int startCityIndex = FindColumnIndex(header, "start_city_id");
            int giverBuildingIndex = FindColumnIndex(header, "giver_building");
            int briefingEventIndex = FindColumnIndex(header, "briefing_event_id");

            if (idIndex < 0 || nameKeyIndex < 0)
            {
                Debug.LogError($"[SPJ] Missing required columns in {csvFile}");
                return quests;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id)) continue;

                string nameKey = GetField(rows[i], nameKeyIndex).Trim();
                string descKey = GetField(rows[i], descKeyIndex).Trim();
                string branchId = GetField(rows[i], branchIdIndex).Trim();
                string startCityId = startCityIndex >= 0 ? GetField(rows[i], startCityIndex).Trim() : "";
                TryParseInt(GetField(rows[i], orderIndex), out int orderInBranch);

                string giverBuildingRaw = giverBuildingIndex >= 0 ? GetField(rows[i], giverBuildingIndex).Trim() : "";
                BuildingType giverBuilding = Enum.TryParse<BuildingType>(giverBuildingRaw, ignoreCase: true, out var parsedBuilding)
                    ? parsedBuilding
                    : BuildingType.Unknown;
                string briefingEventId = briefingEventIndex >= 0 ? GetField(rows[i], briefingEventIndex).Trim() : "";

                LocalizedString nameLS = MakeLocalizedString(nameKey, locTableName);
                LocalizedString descLS = MakeLocalizedString(descKey, locTableName);

                QuestBranch branch = TryLookup(branchMap, branchId, QuestBranch.None, csvFile, i + 1, "branch_id");
                Sprite icon = LoadSprite(SPRITES_FOLDER, rows[i], iconIndex, "Quest");

                branchLocMap.TryGetValue(branch, out var branchLoc);
                LocalizedString branchNameLS = MakeLocalizedString(branchLoc.nameKey, locTableName);
                LocalizedString branchDescLS = MakeLocalizedString(branchLoc.descKey, locTableName);

                List<QuestStageData> builtStages = BuildStages(id, stages, locTableName);
                List<QuestRewardData> builtRewards = BuildRewards(id, rewards);

                failConditions.TryGetValue(id, out List<QuestFailCondition> conditions);
                failConsequences.TryGetValue(id, out List<EventOutcomeEntry> consequences);

                QuestData asset = LoadOrCreateAsset<QuestData>(QUESTS_FOLDER, id);
                asset.ApplyImport(id, nameLS, descLS, icon, branch, orderInBranch, startCityId,
                    branchNameLS, branchDescLS,
                    builtStages, builtRewards, conditions, consequences,
                    giverBuilding, briefingEventId);
                EditorUtility.SetDirty(asset);
                quests.Add(asset);
            }

            return quests;
        }

        private static List<QuestStageData> BuildStages(
            string questId,
            Dictionary<string, List<QuestStagesTable.StageRaw>> stagesByQuest,
            string locTableName)
        {
            List<QuestStageData> result = new();

            if (!stagesByQuest.TryGetValue(questId, out var rawStages))
                return result;

            foreach (var raw in rawStages)
            {
                LocalizedString descLS = MakeLocalizedString(raw.DescriptionKey, locTableName);
                LocalizedString narrativeLS = MakeLocalizedString(raw.NarrativeKey, locTableName);

                QuestStageCondition autoCondition = raw.AutoConditionType != QuestStageConditionType.None
                    ? new QuestStageCondition(raw.AutoConditionType, raw.AutoConditionParam, raw.AutoConditionValue)
                    : default;

                result.Add(new QuestStageData(raw.Id, descLS, narrativeLS, raw.TriggerEventIds, autoCondition));
            }

            return result;
        }

        private static List<QuestRewardData> BuildRewards(
            string questId,
            Dictionary<string, List<QuestRewardsTable.RewardRaw>> rewardsByQuest)
        {
            List<QuestRewardData> result = new();

            if (!rewardsByQuest.TryGetValue(questId, out var rawRewards))
                return result;

            foreach (var raw in rawRewards)
                result.Add(new QuestRewardData(raw.Type, raw.Target, raw.Value));

            return result;
        }
    }
}

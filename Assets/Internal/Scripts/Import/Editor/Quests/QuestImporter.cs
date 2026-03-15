using System;
using System.Collections.Generic;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Import.Editor.Quests.Generators;
using Internal.Scripts.Import.Editor.Quests.Tables;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.Quests.Generated;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Quests
{
    public static class QuestImporter
    {
        private const string LOCALIZATION_TABLE_NAME = "Quests";
        private const string QUESTS_FOLDER = GENERATED_DATA_FOLDER + "/Quests";

        [MenuItem("SPJ/Import/Quests")]
        public static void ImportAll()
        {
            if (IsCompiling()) return;

            try
            {
                QuestBranchGenerator.Generate();
                QuestRewardTypeGenerator.Generate();
                QuestFailConditionTypeGenerator.Generate();
                QuestStageConditionTypeGenerator.Generate();

                EnsureAssetFolder(QUESTS_FOLDER);
                EnsureAssetFolder(DATABASES_FOLDER);

                var branchRows = CsvReader.ReadFile(CsvPath("quest_branches.csv"));
                var branchMap = BuildEnumMap<QuestBranch>(branchRows, "quest_branches.csv", "branch_id", "enum_name");
                var branchLocMap = BuildBranchLocMap(branchRows, branchMap);

                var stages = QuestStagesTable.Read();
                var rewards = QuestRewardsTable.Read();
                var failConditions = QuestFailConditionsTable.Read();
                var failConsequences = QuestFailConsequencesTable.Read();

                var quests = QuestsTable.Import(
                    branchMap, branchLocMap, stages, rewards, failConditions, failConsequences,
                    LOCALIZATION_TABLE_NAME);

                UpdateQuestDatabase(quests);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[SPJ] Quest data import finished.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static Dictionary<QuestBranch, (string nameKey, string descKey)> BuildBranchLocMap(
            List<string[]> rows, Dictionary<string, QuestBranch> branchMap)
        {
            var map = new Dictionary<QuestBranch, (string, string)>();
            if (rows.Count == 0) return map;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "branch_id");
            int nameKeyIndex = FindColumnIndex(header, "name_key");
            int descKeyIndex = FindColumnIndex(header, "description_key");

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (!branchMap.TryGetValue(id, out var branch)) continue;

                string nameKey = nameKeyIndex >= 0 ? GetField(rows[i], nameKeyIndex).Trim() : string.Empty;
                string descKey = descKeyIndex >= 0 ? GetField(rows[i], descKeyIndex).Trim() : string.Empty;
                map[branch] = (nameKey, descKey);
            }

            return map;
        }

        private static void UpdateQuestDatabase(List<QuestData> quests)
        {
            string assetPath = $"{DATABASES_FOLDER}/QuestDatabase.asset";
            QuestDatabase db = LoadOrCreateAsset<QuestDatabase>(assetPath);

            db.ApplyImport(quests);
            EditorUtility.SetDirty(db);
        }
    }
}

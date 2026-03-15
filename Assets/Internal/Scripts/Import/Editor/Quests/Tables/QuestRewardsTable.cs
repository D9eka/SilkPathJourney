using System;
using System.Collections.Generic;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Quests.Generated;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Quests.Tables
{
    public static class QuestRewardsTable
    {
        public struct RewardRaw
        {
            public QuestRewardType Type;
            public string Target;
            public int Value;
        }

        public static Dictionary<string, List<RewardRaw>> Read(string csvFile = "quest_rewards.csv")
        {
            var map = new Dictionary<string, List<RewardRaw>>(StringComparer.Ordinal);

            var rows = CsvReader.ReadFileSafe(CsvPath(csvFile));
            if (rows == null) return map;

            string[] header = rows[0];
            int questIdIndex = FindColumnIndex(header, "quest_id");
            int typeIndex = FindColumnIndex(header, "reward_type");
            int targetIndex = FindColumnIndex(header, "target");
            int valueIndex = FindColumnIndex(header, "value");

            if (questIdIndex < 0 || typeIndex < 0 || valueIndex < 0)
            {
                Debug.LogError($"[SPJ] Missing required columns in {csvFile}");
                return map;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string questId = GetField(rows[i], questIdIndex).Trim();
                if (string.IsNullOrWhiteSpace(questId)) continue;

                string typeStr = GetField(rows[i], typeIndex).Trim();
                if (!Enum.TryParse(ToPascalCase(typeStr), out QuestRewardType rewardType) ||
                    rewardType == QuestRewardType.None)
                {
                    Debug.LogWarning($"[SPJ] Unknown reward type '{typeStr}' in {csvFile} (row {i + 1})");
                    continue;
                }

                TryParseInt(GetField(rows[i], valueIndex), out int value);

                var reward = new RewardRaw
                {
                    Type = rewardType,
                    Target = GetField(rows[i], targetIndex).Trim(),
                    Value = value
                };

                map.GetOrCreateList(questId).Add(reward);
            }

            return map;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Quests.Generated;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Quests.Tables
{
    public static class QuestStagesTable
    {
        public struct StageRaw
        {
            public string Id;
            public string DescriptionKey;
            public string NarrativeKey;
            public string[] TriggerEventIds;
            public QuestStageConditionType AutoConditionType;
            public string AutoConditionParam;
            public int AutoConditionValue;
        }

        public static Dictionary<string, List<StageRaw>> Read(string csvFile = "quest_stages.csv")
        {
            var map = new Dictionary<string, List<StageRaw>>(StringComparer.Ordinal);

            var rows = CsvReader.ReadFileSafe(CsvPath(csvFile));
            if (rows == null) return map;

            string[] header = rows[0];
            int questIdIndex = FindColumnIndex(header, "quest_id");
            int stageIdIndex = FindColumnIndex(header, "stage_id");
            int descKeyIndex = FindColumnIndex(header, "description_key");
            int narrativeKeyIndex = FindColumnIndex(header, "narrative_key");
            int triggerIndex = FindColumnIndex(header, "trigger_event_id");
            int condTypeIndex = FindColumnIndex(header, "auto_condition_type");
            int condParamIndex = FindColumnIndex(header, "auto_condition_param");
            int condValueIndex = FindColumnIndex(header, "auto_condition_value");

            if (questIdIndex < 0 || stageIdIndex < 0)
            {
                Debug.LogError($"[SPJ] Missing required columns in {csvFile}");
                return map;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string questId = GetField(rows[i], questIdIndex).Trim();
                if (string.IsNullOrWhiteSpace(questId)) continue;

                string typeStr = GetField(rows[i], condTypeIndex).Trim();
                QuestStageConditionType condType = QuestStageConditionType.None;
                if (!string.IsNullOrWhiteSpace(typeStr))
                {
                    if (!Enum.TryParse(ToPascalCase(typeStr), out condType) || condType == QuestStageConditionType.None)
                    {
                        Debug.LogWarning($"[SPJ] Unknown condition type '{typeStr}' in {csvFile} (row {i + 1})");
                        condType = QuestStageConditionType.None;
                    }
                }

                TryParseInt(GetField(rows[i], condValueIndex), out int condValue);

                string triggerRaw = GetField(rows[i], triggerIndex).Trim();
                string[] triggerIds = string.IsNullOrWhiteSpace(triggerRaw)
                    ? Array.Empty<string>()
                    : triggerRaw.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToArray();

                var stage = new StageRaw
                {
                    Id = GetField(rows[i], stageIdIndex).Trim(),
                    DescriptionKey = GetField(rows[i], descKeyIndex).Trim(),
                    NarrativeKey = narrativeKeyIndex >= 0 ? GetField(rows[i], narrativeKeyIndex).Trim() : "",
                    TriggerEventIds = triggerIds,
                    AutoConditionType = condType,
                    AutoConditionParam = GetField(rows[i], condParamIndex).Trim(),
                    AutoConditionValue = condValue
                };

                map.GetOrCreateList(questId).Add(stage);
            }

            return map;
        }
    }
}

using System;
using System.Collections.Generic;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.Quests.Generated;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Quests.Tables
{
    public static class QuestFailConditionsTable
    {
        public static Dictionary<string, List<QuestFailCondition>> Read(
            string csvFile = "quest_fail_conditions.csv")
        {
            var map = new Dictionary<string, List<QuestFailCondition>>(StringComparer.Ordinal);

            var rows = CsvReader.ReadFileSafe(CsvPath(csvFile));
            if (rows == null) return map;

            string[] header = rows[0];
            int questIdIndex = FindColumnIndex(header, "quest_id");
            int typeIndex = FindColumnIndex(header, "type");
            int paramIndex = FindColumnIndex(header, "param");
            int valueIndex = FindColumnIndex(header, "value");

            if (questIdIndex < 0 || typeIndex < 0 || valueIndex < 0)
            {
                Debug.LogError($"[SPJ] Missing columns in {csvFile}");
                return map;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string questId = GetField(rows[i], questIdIndex).Trim();
                if (string.IsNullOrWhiteSpace(questId)) continue;

                string typeStr = GetField(rows[i], typeIndex).Trim();
                string param = GetField(rows[i], paramIndex).Trim();
                TryParseInt(GetField(rows[i], valueIndex), out int value);

                if (!TryParseConditionType(typeStr, out QuestFailConditionType condType))
                {
                    Debug.LogWarning($"[SPJ] Unknown condition type '{typeStr}' in {csvFile} (row {i + 1})");
                    continue;
                }

                map.GetOrCreateList(questId).Add(new QuestFailCondition(condType, param, value));
            }

            return map;
        }

        private static bool TryParseConditionType(string raw, out QuestFailConditionType result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(raw)) return false;
            return Enum.TryParse(ToPascalCase(raw), out result) && result != QuestFailConditionType.None;
        }
    }
}

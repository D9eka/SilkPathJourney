using System;
using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Quests.Tables
{
    public static class QuestFailConsequencesTable
    {
        public static Dictionary<string, List<EventOutcomeEntry>> Read(
            string csvFile = "quest_fail_consequences.csv")
        {
            var map = new Dictionary<string, List<EventOutcomeEntry>>(StringComparer.Ordinal);

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
                TryParseFloat(GetField(rows[i], valueIndex), out float value);

                if (!TryParseOutcomeType(typeStr, out EventOutcomeType outcomeType))
                {
                    Debug.LogWarning($"[SPJ] Unknown outcome type '{typeStr}' in {csvFile} (row {i + 1})");
                    continue;
                }

                map.GetOrCreateList(questId).Add(new EventOutcomeEntry(outcomeType, param, value));
            }

            return map;
        }

        private static bool TryParseOutcomeType(string raw, out EventOutcomeType result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(raw)) return false;
            return Enum.TryParse(ToPascalCase(raw), out result) && result != EventOutcomeType.None;
        }
    }
}

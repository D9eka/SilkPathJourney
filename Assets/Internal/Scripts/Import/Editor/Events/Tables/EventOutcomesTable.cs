using System;
using System.Collections.Generic;
using System.IO;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Events.Tables
{
    public static class EventOutcomesTable
    {
        public static Dictionary<string, Dictionary<int, List<EventOutcomeEntry>>> Read()
        {
            Dictionary<string, Dictionary<int, List<EventOutcomeEntry>>> map =
                new(StringComparer.Ordinal);

            string csvPath = CsvPath("event_choice_outcomes.csv");
            if (!File.Exists(csvPath))
            {
                Debug.LogWarning("[SPJ] event_choice_outcomes.csv not found.");
                return map;
            }

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count == 0) return map;

            string[] header = rows[0];
            int eventIdIndex = FindColumnIndex(header, "event_id");
            int choiceIndexIndex = FindColumnIndex(header, "choice_index");
            int typeIndex = FindColumnIndex(header, "type");
            int paramIndex = FindColumnIndex(header, "param");
            int valueIndex = FindColumnIndex(header, "value");
            if (eventIdIndex < 0 || choiceIndexIndex < 0 || typeIndex < 0 || valueIndex < 0)
            {
                Debug.LogError("[SPJ] Missing columns in event_choice_outcomes.csv");
                return map;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string eventId = GetField(rows[i], eventIdIndex).Trim();
                if (string.IsNullOrWhiteSpace(eventId)) continue;

                TryParseInt(GetField(rows[i], choiceIndexIndex), out int choiceIndex);
                string typeStr = GetField(rows[i], typeIndex).Trim();
                string param = paramIndex >= 0 ? GetField(rows[i], paramIndex).Trim() : "";
                TryParseFloat(GetField(rows[i], valueIndex), out float value);

                if (!TryParseOutcomeType(typeStr, out EventOutcomeType outcomeType))
                {
                    Debug.LogWarning($"[SPJ] Unknown outcome type '{typeStr}' in event_choice_outcomes.csv (row {i + 1})");
                    continue;
                }

                if (!map.TryGetValue(eventId, out Dictionary<int, List<EventOutcomeEntry>> byChoice))
                {
                    byChoice = new Dictionary<int, List<EventOutcomeEntry>>();
                    map[eventId] = byChoice;
                }

                if (!byChoice.TryGetValue(choiceIndex, out List<EventOutcomeEntry> oList))
                {
                    oList = new List<EventOutcomeEntry>();
                    byChoice[choiceIndex] = oList;
                }

                oList.Add(new EventOutcomeEntry(outcomeType, param, value));
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

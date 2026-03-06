using System;
using System.Collections.Generic;
using System.IO;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Events.Tables
{
    public static class EventOutcomesTable
    {
        public struct OutcomeBranch
        {
            public List<EventOutcomeEntry> Success;
            public List<EventOutcomeEntry> Fail;
        }

        public static Dictionary<string, Dictionary<int, OutcomeBranch>> Read(
            string csvFile = "event_choice_outcomes.csv")
        {
            Dictionary<string, Dictionary<int, OutcomeBranch>> map =
                new(StringComparer.Ordinal);

            string csvPath = CsvPath(csvFile);
            if (!File.Exists(csvPath))
            {
                Debug.LogWarning($"[SPJ] {csvFile} not found.");
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
            int branchIndex = FindColumnIndex(header, "branch");
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
                string branch = branchIndex >= 0 ? GetField(rows[i], branchIndex).Trim().ToLowerInvariant() : "";

                if (!TryParseOutcomeType(typeStr, out EventOutcomeType outcomeType))
                {
                    Debug.LogWarning($"[SPJ] Unknown outcome type '{typeStr}' in {csvFile} (row {i + 1})");
                    continue;
                }

                if (!map.TryGetValue(eventId, out Dictionary<int, OutcomeBranch> byChoice))
                {
                    byChoice = new Dictionary<int, OutcomeBranch>();
                    map[eventId] = byChoice;
                }

                if (!byChoice.TryGetValue(choiceIndex, out OutcomeBranch ob))
                {
                    ob = new OutcomeBranch
                    {
                        Success = new List<EventOutcomeEntry>(),
                        Fail = new List<EventOutcomeEntry>()
                    };
                }

                var entry = new EventOutcomeEntry(outcomeType, param, value);

                if (branch == "fail")
                    ob.Fail.Add(entry);
                else
                    ob.Success.Add(entry);

                byChoice[choiceIndex] = ob;
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

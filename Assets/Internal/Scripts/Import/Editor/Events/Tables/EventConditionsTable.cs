using System;
using System.Collections.Generic;
using System.IO;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Events.Tables
{
    public static class EventConditionsTable
    {
        public static void Read(
            out Dictionary<string, List<EventCondition>> eventConditions,
            out Dictionary<string, Dictionary<int, List<EventCondition>>> choiceConditions)
        {
            eventConditions = new Dictionary<string, List<EventCondition>>(StringComparer.Ordinal);
            choiceConditions = new Dictionary<string, Dictionary<int, List<EventCondition>>>(StringComparer.Ordinal);

            string csvPath = CsvPath("event_conditions.csv");
            if (!File.Exists(csvPath))
            {
                Debug.LogWarning("[SPJ] event_conditions.csv not found.");
                return;
            }

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count == 0) return;

            string[] header = rows[0];
            int eventIdIndex = FindColumnIndex(header, "event_id");
            int choiceIndexIndex = FindColumnIndex(header, "choice_index");
            int typeIndex = FindColumnIndex(header, "type");
            int paramIndex = FindColumnIndex(header, "param");
            int valueIndex = FindColumnIndex(header, "value");
            if (eventIdIndex < 0 || typeIndex < 0 || valueIndex < 0)
            {
                Debug.LogError("[SPJ] Missing columns in event_conditions.csv");
                return;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string eventId = GetField(rows[i], eventIdIndex).Trim();
                if (string.IsNullOrWhiteSpace(eventId)) continue;

                string choiceIndexStr = choiceIndexIndex >= 0 ? GetField(rows[i], choiceIndexIndex).Trim() : "";
                string typeStr = GetField(rows[i], typeIndex).Trim();
                string param = paramIndex >= 0 ? GetField(rows[i], paramIndex).Trim() : "";
                TryParseFloat(GetField(rows[i], valueIndex), out float value);

                if (!TryParseConditionType(typeStr, out EventConditionType condType))
                {
                    Debug.LogWarning($"[SPJ] Unknown condition type '{typeStr}' in event_conditions.csv (row {i + 1})");
                    continue;
                }

                EventCondition condition = new EventCondition(condType, param, value);

                if (string.IsNullOrWhiteSpace(choiceIndexStr))
                {
                    if (!eventConditions.TryGetValue(eventId, out List<EventCondition> list))
                    {
                        list = new List<EventCondition>();
                        eventConditions[eventId] = list;
                    }
                    list.Add(condition);
                }
                else
                {
                    TryParseInt(choiceIndexStr, out int choiceIdx);
                    if (!choiceConditions.TryGetValue(eventId, out Dictionary<int, List<EventCondition>> byChoice))
                    {
                        byChoice = new Dictionary<int, List<EventCondition>>();
                        choiceConditions[eventId] = byChoice;
                    }
                    if (!byChoice.TryGetValue(choiceIdx, out List<EventCondition> cList))
                    {
                        cList = new List<EventCondition>();
                        byChoice[choiceIdx] = cList;
                    }
                    cList.Add(condition);
                }
            }
        }

        private static bool TryParseConditionType(string raw, out EventConditionType result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(raw)) return false;
            return Enum.TryParse(ToPascalCase(raw), out result) && result != EventConditionType.None;
        }
    }
}

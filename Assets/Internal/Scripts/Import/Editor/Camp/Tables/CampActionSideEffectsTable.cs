using System;
using System.Collections.Generic;
using Internal.Scripts.Camp;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Camp.Tables
{
    public static class CampActionSideEffectsTable
    {
        public static Dictionary<string, List<RepeatSideEffect>> Read(
            string csvFile = "camp_action_side_effects.csv")
        {
            Dictionary<string, List<RepeatSideEffect>> map =
                new(StringComparer.Ordinal);

            var rows = CsvReader.ReadFileSafe(CsvPath(csvFile));
            if (rows == null || rows.Count <= 1)
                return map;

            string[] header = rows[0];
            int actionIdIndex = FindColumnIndex(header, "action_id");
            int repeatDayIndex = FindColumnIndex(header, "repeat_day");
            int resourceIndex = FindColumnIndex(header, "resource");
            int valueIndex = FindColumnIndex(header, "value");
            int eventChanceIndex = FindColumnIndex(header, "event_chance");

            if (actionIdIndex < 0 || repeatDayIndex < 0 || resourceIndex < 0 || valueIndex < 0)
            {
                Debug.LogError("[SPJ] Missing columns in camp_action_side_effects.csv");
                return map;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string actionId = GetField(rows[i], actionIdIndex).Trim();
                if (string.IsNullOrWhiteSpace(actionId)) continue;

                string resourceStr = GetField(rows[i], resourceIndex).Trim();
                if (!Enum.TryParse(resourceStr, out EventOutcomeType resource))
                {
                    Debug.LogWarning($"[SPJ] Unknown resource '{resourceStr}' in {csvFile} (row {i + 1})");
                    continue;
                }

                TryParseInt(GetField(rows[i], repeatDayIndex), out int repeatDay);
                TryParseFloat(GetField(rows[i], valueIndex), out float value);

                float eventChance = 0f;
                if (eventChanceIndex >= 0)
                    TryParseFloat(GetField(rows[i], eventChanceIndex), out eventChance);

                var entry = new RepeatSideEffect
                {
                    RepeatDay = repeatDay,
                    Resource = resource,
                    Value = value,
                    EventChance = eventChance
                };

                if (!map.TryGetValue(actionId, out var list))
                {
                    list = new List<RepeatSideEffect>();
                    map[actionId] = list;
                }

                list.Add(entry);
            }

            return map;
        }
    }
}

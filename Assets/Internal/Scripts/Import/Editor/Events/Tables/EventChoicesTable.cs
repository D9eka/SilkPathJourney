using System;
using System.Collections.Generic;
using System.IO;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Events.Tables
{
    public static class EventChoicesTable
    {
        public struct ChoiceRaw
        {
            public int Index;
            public string NameKey;
            public string ResultKey;
        }

        public static Dictionary<string, List<ChoiceRaw>> Read()
        {
            Dictionary<string, List<ChoiceRaw>> map = new(StringComparer.Ordinal);
            string csvPath = CsvPath("event_choices.csv");
            if (!File.Exists(csvPath))
            {
                Debug.LogWarning("[SPJ] event_choices.csv not found.");
                return map;
            }

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count == 0) return map;

            string[] header = rows[0];
            int eventIdIndex = FindColumnIndex(header, "event_id");
            int choiceIndexIndex = FindColumnIndex(header, "choice_index");
            int nameKeyIndex = FindColumnIndex(header, "name_key");
            int resultKeyIndex = FindColumnIndex(header, "result_key");
            if (eventIdIndex < 0 || choiceIndexIndex < 0 || nameKeyIndex < 0)
            {
                Debug.LogError("[SPJ] Missing columns in event_choices.csv");
                return map;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string eventId = GetField(rows[i], eventIdIndex).Trim();
                if (string.IsNullOrWhiteSpace(eventId)) continue;

                TryParseInt(GetField(rows[i], choiceIndexIndex), out int choiceIndex);
                string nameKey = GetField(rows[i], nameKeyIndex).Trim();
                string resultKey = resultKeyIndex >= 0 ? GetField(rows[i], resultKeyIndex).Trim() : "";

                if (!map.TryGetValue(eventId, out List<ChoiceRaw> list))
                {
                    list = new List<ChoiceRaw>();
                    map[eventId] = list;
                }

                list.Add(new ChoiceRaw { Index = choiceIndex, NameKey = nameKey, ResultKey = resultKey });
            }

            foreach (var list in map.Values)
                list.Sort((a, b) => a.Index.CompareTo(b.Index));

            return map;
        }
    }
}

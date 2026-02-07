using System;
using System.Collections.Generic;
using System.IO;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Events.Tables
{
    public static class EventTypesTable
    {
        public static Dictionary<string, string> Read()
        {
            Dictionary<string, string> map = new(StringComparer.Ordinal);
            string csvPath = CsvPath("event_types.csv");
            if (!File.Exists(csvPath))
            {
                Debug.LogWarning("[SPJ] event_types.csv not found.");
                return map;
            }

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count == 0) return map;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "event_type_id");
            int nameKeyIndex = FindColumnIndex(header, "name_key");
            if (idIndex < 0 || nameKeyIndex < 0)
            {
                Debug.LogError("[SPJ] Missing columns in event_types.csv");
                return map;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                string nameKey = GetField(rows[i], nameKeyIndex).Trim();
                if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(nameKey))
                    map[id] = nameKey;
            }

            return map;
        }
    }
}

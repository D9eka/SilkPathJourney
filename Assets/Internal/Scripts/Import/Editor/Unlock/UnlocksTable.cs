using System;
using System.Collections.Generic;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Meta.Unlocks;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Unlock
{
    public static class UnlocksTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/Unlocks";
        private const string LOC_TABLE = "Player";

        public static List<UnlockData> Import()
        {
            List<string[]> rows = CsvReader.ReadFile(CsvPath("unlocks.csv"));
            List<UnlockData> result = new();

            if (rows.Count == 0)
                return result;

            string[] header = rows[0];
            int idIdx = FindColumnIndex(header, "unlock_id");
            int nameKeyIdx = FindColumnIndex(header, "name_key");
            int descKeyIdx = FindColumnIndex(header, "description_key");
            int categoryIdx = FindColumnIndex(header, "category");
            int costIdx = FindColumnIndex(header, "cost");
            int iconIdx = FindColumnIndex(header, "icon");

            if (idIdx < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in unlocks.csv");
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIdx).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                string categoryRaw = GetField(rows[i], categoryIdx).Trim();
                if (!Enum.TryParse(categoryRaw, true, out UnlockCategory category))
                {
                    Debug.LogWarning($"[SPJ] Unknown UnlockCategory '{categoryRaw}' in unlocks.csv (row {i + 1})");
                    category = UnlockCategory.Background;
                }

                TryParseInt(GetField(rows[i], costIdx), out int cost);

                var nameKey = MakeLocalizedString(GetField(rows[i], nameKeyIdx).Trim(), LOC_TABLE);
                var descKey = MakeLocalizedString(GetField(rows[i], descKeyIdx).Trim(), LOC_TABLE);
                var icon = LoadSprite(OUTPUT_FOLDER, rows[i], iconIdx, id);

                UnlockData asset = LoadOrCreateAsset<UnlockData>(OUTPUT_FOLDER, id);
                asset.ApplyImport(id, category, nameKey, descKey, cost, icon);
                EditorUtility.SetDirty(asset);
                result.Add(asset);
            }

            return result;
        }
    }
}

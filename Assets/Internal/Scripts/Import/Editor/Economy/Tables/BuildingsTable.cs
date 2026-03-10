using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.UI.Screens.Core.Config;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy.Tables
{
    public static class BuildingsTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/Buildings";

        public static List<BuildingData> Import(
            string locTableName)
        {
            string csvPath = CsvPath("buildings.csv");
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            List<BuildingData> buildings = new();

            if (rows.Count == 0)
                return buildings;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "building_id");
            int enumIndex = FindColumnIndex(header, "enum_name");
            int nameIndex = FindColumnIndex(header, "name_key");
            int descIndex = FindColumnIndex(header, "description_key");
            int screenIndex = FindColumnIndex(header, "interaction_screen");
            if (idIndex < 0 || enumIndex < 0 || nameIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in buildings.csv");
                return buildings;
            }

            EnsureAssetFolder(OUTPUT_FOLDER);

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                string enumName = GetField(rows[i], enumIndex).Trim();
                if (!Enum.TryParse(enumName, true, out BuildingType type))
                {
                    Debug.LogWarning($"[SPJ] Unknown BuildingType '{enumName}' in buildings.csv (row {i + 1})");
                    type = BuildingType.Unknown;
                }

                ScreenId screen = ScreenId.None;
                if (screenIndex >= 0)
                {
                    string screenStr = GetField(rows[i], screenIndex).Trim();
                    if (!string.IsNullOrWhiteSpace(screenStr))
                        Enum.TryParse(screenStr, true, out screen);
                }

                var descLS = descIndex >= 0
                    ? MakeLocalizedString(GetField(rows[i], descIndex).Trim(), locTableName)
                    : new LocalizedString();

                BuildingData asset = LoadOrCreateAsset<BuildingData>(OUTPUT_FOLDER, id);

                asset.ApplyImport(
                    id,
                    type,
                    MakeLocalizedString(GetField(rows[i], nameIndex).Trim(), locTableName),
                    descLS,
                    screen);

                EditorUtility.SetDirty(asset);
                buildings.Add(asset);
            }

            return buildings;
        }
    }
}

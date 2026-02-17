using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Import.Editor.Core;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy.Tables
{
    public static class CityModifiersTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/CityModifiers";
        private const string ICONS_FOLDER = "Assets/Internal/Sprites/CityModifiers";

        public static List<CityModifierData> Import(
            string locTableName,
            Dictionary<string, LocalizationImporter.LocalizationEntry> locEntries)
        {
            string csvPath = CsvPath("city_modifiers.csv");
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            List<CityModifierData> modifiers = new();

            if (rows.Count == 0)
                return modifiers;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "modifier_id");
            int nameIndex = FindColumnIndex(header, "name_key");
            int descIndex = FindColumnIndex(header, "description_key");
            int iconIndex = FindColumnIndex(header, "icon_name");
            if (idIndex < 0 || nameIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in city_modifiers.csv");
                return modifiers;
            }

            EnsureAssetFolder(OUTPUT_FOLDER);

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                string nameKey = GetField(rows[i], nameIndex).Trim();
                string descKey = descIndex >= 0 ? GetField(rows[i], descIndex).Trim() : string.Empty;

                Sprite icon = null;
                if (iconIndex >= 0)
                {
                    string iconName = GetField(rows[i], iconIndex).Trim();
                    if (!string.IsNullOrWhiteSpace(iconName))
                    {
                        string iconPath = $"{ICONS_FOLDER}/{iconName}.png";
                        icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                        if (icon == null)
                            Debug.LogWarning($"[SPJ] City modifier icon not found: {iconPath}");
                    }
                }

                CityModifierData asset = LoadOrCreateAsset<CityModifierData>(OUTPUT_FOLDER, id);

                asset.ApplyImport(
                    id,
                    MakeLocalizedString(nameKey, locTableName),
                    MakeLocalizedString(descKey, locTableName),
                    icon);

                EditorUtility.SetDirty(asset);
                modifiers.Add(asset);
            }

            return modifiers;
        }
    }
}

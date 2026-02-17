using System.Collections.Generic;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Road.Modifiers;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy.Tables
{
    public static class RoadModifiersTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/RoadModifiers";
        private const string ICONS_FOLDER = "Assets/Internal/Sprites/Modifiers";

        public static List<RoadModifierData> Import(
            string locTableName,
            Dictionary<string, LocalizationImporter.LocalizationEntry> locEntries)
        {
            string csvPath = CsvPath("road_modifiers.csv");
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            List<RoadModifierData> modifiers = new();

            if (rows.Count == 0)
                return modifiers;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "modifier_id");
            int nameIndex = FindColumnIndex(header, "name_key");
            int descIndex = FindColumnIndex(header, "description_key");
            int iconIndex = FindColumnIndex(header, "icon_name");
            if (idIndex < 0 || nameIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in road_modifiers.csv");
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
                            Debug.LogWarning($"[SPJ] Road modifier icon not found: {iconPath}");
                    }
                }

                RoadModifierData asset = LoadOrCreateAsset<RoadModifierData>(OUTPUT_FOLDER, id);

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

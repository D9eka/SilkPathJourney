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
            string locTableName)
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
            int pricePctIndex = FindColumnIndex(header, "price_pct");
            int dangerPctIndex = FindColumnIndex(header, "danger_pct");
            int conflictGroupIndex = FindColumnIndex(header, "conflict_group");
            int biomeIndex = FindColumnIndex(header, "biome_restriction");
            int minDurIndex = FindColumnIndex(header, "min_duration");
            int maxDurIndex = FindColumnIndex(header, "max_duration");
            int cascadeRoadIndex = FindColumnIndex(header, "cascade_road_id");
            int cascadeChanceIndex = FindColumnIndex(header, "cascade_chance");
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
                string descKey = GetField(rows[i], descIndex).Trim();
                Sprite icon = LoadSprite(ICONS_FOLDER, rows[i], iconIndex, "City modifier");

                CityModifierData asset = LoadOrCreateAsset<CityModifierData>(OUTPUT_FOLDER, id);

                TryParseFloat(GetField(rows[i], pricePctIndex), out float pricePct);
                TryParseFloat(GetField(rows[i], dangerPctIndex), out float dangerPct);
                TryParseInt(GetField(rows[i], minDurIndex), out int minDur);
                TryParseInt(GetField(rows[i], maxDurIndex), out int maxDur);
                TryParseFloat(GetField(rows[i], cascadeChanceIndex), out float cascadeChance);

                asset.ApplyImport(
                    id,
                    MakeLocalizedString(nameKey, locTableName),
                    MakeLocalizedString(descKey, locTableName),
                    icon,
                    pricePct,
                    dangerPct,
                    GetField(rows[i], conflictGroupIndex).Trim(),
                    GetField(rows[i], biomeIndex).Trim(),
                    minDur,
                    maxDur,
                    GetField(rows[i], cascadeRoadIndex).Trim(),
                    cascadeChance);

                EditorUtility.SetDirty(asset);
                modifiers.Add(asset);
            }

            return modifiers;
        }
    }
}

using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy.Tables
{
    public static class CitiesTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/Cities";

        public static List<CityData> Import(
            Dictionary<string, CityType> cityTypeMap,
            Dictionary<string, CultureId> cultureMap,
            Dictionary<string, Biome> biomeMap,
            string locTableName,
            Dictionary<string, LocalizationImporter.LocalizationEntry> locEntries)
        {
            string csvPath = CsvPath("cities.csv");
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            List<CityData> cities = new();

            if (rows.Count == 0)
                return cities;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "city_id");
            int nodeIndex = FindColumnIndex(header, "node_id");
            int nameIndex = FindColumnIndex(header, "name_key");
            int descIndex = FindColumnIndex(header, "description_key");
            int typeIndex = FindColumnIndex(header, "city_type_id");
            int primaryCultureIndex = FindColumnIndex(header, "primary_culture_id");
            int secondaryCultureIndex = FindColumnIndex(header, "secondary_culture_id");
            int marketScaleIndex = FindColumnIndex(header, "market_scale");
            int hasPortIndex = FindColumnIndex(header, "has_port");
            int biomeIndex = FindColumnIndex(header, "biome_id");
            if (idIndex < 0 || nodeIndex < 0 || nameIndex < 0 || typeIndex < 0 ||
                primaryCultureIndex < 0 || secondaryCultureIndex < 0 || marketScaleIndex < 0 ||
                hasPortIndex < 0 || biomeIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in cities.csv");
                return cities;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                string typeId = GetField(rows[i], typeIndex).Trim();
                if (!cityTypeMap.TryGetValue(typeId, out CityType type))
                {
                    Debug.LogWarning($"[SPJ] Unknown city_type_id '{typeId}' in cities.csv (row {i + 1})");
                    type = CityType.Unknown;
                }

                TryParseFloat(GetField(rows[i], marketScaleIndex), out float marketScale);
                bool hasPort = ParseBool(GetField(rows[i], hasPortIndex));

                string biomeId = GetField(rows[i], biomeIndex).Trim();
                if (!biomeMap.TryGetValue(biomeId, out Biome biome))
                {
                    if (!string.IsNullOrEmpty(biomeId))
                        Debug.LogWarning($"[SPJ] Unknown biome_id '{biomeId}' in cities.csv (row {i + 1})");
                    biome = Biome.Unknown;
                }

                CityData asset = LoadOrCreateAsset<CityData>(OUTPUT_FOLDER, id);

                string descKey = descIndex >= 0 ? GetField(rows[i], descIndex).Trim() : string.Empty;

                asset.ApplyImport(
                    id,
                    GetField(rows[i], nodeIndex).Trim(),
                    type,
                    ParseCulture(GetField(rows[i], primaryCultureIndex), cultureMap, i + 1, "primary_culture_id"),
                    ParseCulture(GetField(rows[i], secondaryCultureIndex), cultureMap, i + 1, "secondary_culture_id"),
                    marketScale,
                    hasPort,
                    biome,
                    MakeLocalizedString(GetField(rows[i], nameIndex).Trim(), locTableName),
                    MakeLocalizedString(descKey, locTableName));

                EditorUtility.SetDirty(asset);
                cities.Add(asset);
            }

            return cities;
        }

        private static CultureId ParseCulture(
            string value, Dictionary<string, CultureId> cultureMap, int row, string column)
        {
            string id = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(id))
                return CultureId.None;

            if (!cultureMap.TryGetValue(id, out CultureId culture))
            {
                Debug.LogWarning($"[SPJ] Unknown {column} '{id}' in cities.csv (row {row})");
                return CultureId.None;
            }

            return culture;
        }
    }
}

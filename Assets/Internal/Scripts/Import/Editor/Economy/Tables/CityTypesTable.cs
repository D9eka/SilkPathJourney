using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy.Tables
{
    public static class CityTypesTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/CityTypes";

        public static List<CityTypeData> Import(
            Dictionary<string, CityType> typeMap,
            Dictionary<string, List<CityTypeData.CategoryCoef>> coefs,
            Dictionary<string, List<CityTypeData.CategoryStockProfile>> profiles,
            string locTableName,
            Dictionary<string, LocalizationImporter.LocalizationEntry> locEntries)
        {
            string csvPath = CsvPath("city_types.csv");
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            List<CityTypeData> cityTypes = new();

            if (rows.Count == 0)
                return cityTypes;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "city_type_id");
            int nameIndex = FindColumnIndex(header, "name_key");
            int descIndex = FindColumnIndex(header, "description_key");
            int moneyIndex = FindColumnIndex(header, "CityMoneyIncomePerScale");
            if (idIndex < 0 || nameIndex < 0 || moneyIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in city_types.csv (expected CityMoneyIncomePerScale)");
                return cityTypes;
            }

            bool anyProfilesLoaded = profiles.Count > 0;
            int missingProfilesCount = 0;
            int missingCoefsCount = 0;

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                if (!typeMap.TryGetValue(id, out CityType type))
                {
                    Debug.LogWarning($"[SPJ] Unknown city_type_id '{id}' in city_types.csv (row {i + 1})");
                    type = CityType.Unknown;
                }

                CityTypeData asset = LoadOrCreateAsset<CityTypeData>(OUTPUT_FOLDER, id);

                TryParseInt(GetField(rows[i], moneyIndex), out int moneyPerScale);

                List<CityTypeData.CategoryCoef> coefList =
                    coefs.TryGetValue(id, out List<CityTypeData.CategoryCoef> cl)
                        ? new List<CityTypeData.CategoryCoef>(cl)
                        : new List<CityTypeData.CategoryCoef>();
                if (coefList.Count == 0)
                    missingCoefsCount++;

                List<CityTypeData.CategoryStockProfile> profileList =
                    profiles.TryGetValue(id, out List<CityTypeData.CategoryStockProfile> pl)
                        ? new List<CityTypeData.CategoryStockProfile>(pl)
                        : new List<CityTypeData.CategoryStockProfile>();
                if (profileList.Count == 0)
                    missingProfilesCount++;

                if (coefList.Count > 0)
                {
                    HashSet<ItemType> existing = new();
                    foreach (CityTypeData.CategoryStockProfile profile in profileList)
                        existing.Add(profile.Category);

                    foreach (CityTypeData.CategoryCoef coef in coefList)
                    {
                        if (existing.Contains(coef.Category))
                            continue;

                        profileList.Add(new CityTypeData.CategoryStockProfile
                        {
                            Category = coef.Category,
                            DesiredPerScale = 0f,
                            DailyNet = 0f,
                            EquilibriumPull = 0f
                        });
                    }
                }

                string descKey = descIndex >= 0 ? GetField(rows[i], descIndex).Trim() : string.Empty;

                asset.ApplyImport(
                    type,
                    MakeLocalizedString(GetField(rows[i], nameIndex).Trim(), locTableName),
                    MakeLocalizedString(descKey, locTableName),
                    moneyPerScale,
                    coefList,
                    profileList);

                EditorUtility.SetDirty(asset);
                cityTypes.Add(asset);
            }

            if (missingCoefsCount > 0)
                Debug.LogWarning($"[SPJ] Missing category coefs for {missingCoefsCount} city types. Check city_type_category_coefs.csv.");

            if (!anyProfilesLoaded && cityTypes.Count > 0)
                Debug.LogWarning("[SPJ] No stock profiles loaded. Defaults from category coefs were applied.");
            else if (missingProfilesCount > 0)
                Debug.LogWarning($"[SPJ] Missing stock profiles for {missingProfilesCount} city types. Defaults from category coefs were applied.");

            return cityTypes;
        }
    }
}

using System;
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
            string locTableName)
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
            int tariffIndex = FindColumnIndex(header, "base_tariff_pct");
            if (idIndex < 0 || nameIndex < 0 || moneyIndex < 0 || tariffIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in city_types.csv (expected city_type_id, name_key, CityMoneyIncomePerScale, base_tariff_pct)");
                return cityTypes;
            }

            ItemType[] allCategories = BuildCategoryList();
            HashSet<string> coefCoverage = BuildCategoryCoverage(coefs);
            HashSet<string> profileCoverage = BuildCategoryCoverage(profiles);

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                CityType type = TryLookup(typeMap, id, CityType.Unknown, "city_types.csv", i + 1, "city_type_id");

                CityTypeData asset = LoadOrCreateAsset<CityTypeData>(OUTPUT_FOLDER, id);

                TryParseInt(GetField(rows[i], moneyIndex), out int moneyPerScale);
                TryParseFloat(GetField(rows[i], tariffIndex), out float baseTariffPct);

                List<CityTypeData.CategoryCoef> coefList =
                    coefs.TryGetValue(id, out List<CityTypeData.CategoryCoef> cl)
                        ? new List<CityTypeData.CategoryCoef>(cl)
                        : new List<CityTypeData.CategoryCoef>();

                List<CityTypeData.CategoryStockProfile> profileList =
                    profiles.TryGetValue(id, out List<CityTypeData.CategoryStockProfile> pl)
                        ? new List<CityTypeData.CategoryStockProfile>(pl)
                        : new List<CityTypeData.CategoryStockProfile>();

                foreach (ItemType category in allCategories)
                {
                    string key = $"{id}|{category}";
                    if (!coefCoverage.Contains(key))
                        Debug.LogWarning($"[SPJ Economy] Missing category coef: city_type='{id}', category='{category}' in city_type_category_coefs.csv");
                    if (!profileCoverage.Contains(key))
                        Debug.LogWarning($"[SPJ Economy] Missing stock profile: city_type='{id}', category='{category}' in city_type_category_stock_profile.csv");
                }

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

                string descKey = GetField(rows[i], descIndex).Trim();

                asset.ApplyImport(
                    type,
                    MakeLocalizedString(GetField(rows[i], nameIndex).Trim(), locTableName),
                    MakeLocalizedString(descKey, locTableName),
                    moneyPerScale,
                    baseTariffPct,
                    coefList,
                    profileList);

                EditorUtility.SetDirty(asset);
                cityTypes.Add(asset);
            }

            return cityTypes;
        }

        private static ItemType[] BuildCategoryList()
        {
            ItemType[] all = (ItemType[])Enum.GetValues(typeof(ItemType));
            List<ItemType> result = new();
            foreach (ItemType t in all)
            {
                if (t != ItemType.Unknown)
                    result.Add(t);
            }

            return result.ToArray();
        }

        private static HashSet<string> BuildCategoryCoverage<T>(Dictionary<string, List<T>> data)
            where T : struct
        {
            HashSet<string> coverage = new();
            foreach (KeyValuePair<string, List<T>> kvp in data)
            {
                foreach (T entry in kvp.Value)
                {
                    ItemType category = default;
                    if (entry is CityTypeData.CategoryCoef coef)
                        category = coef.Category;
                    else if (entry is CityTypeData.CategoryStockProfile profile)
                        category = profile.Category;

                    if (category != ItemType.Unknown)
                        coverage.Add($"{kvp.Key}|{category}");
                }
            }

            return coverage;
        }
    }
}

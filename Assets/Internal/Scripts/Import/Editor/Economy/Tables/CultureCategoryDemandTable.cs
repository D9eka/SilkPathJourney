using System;
using System.Collections.Generic;
using System.IO;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy.Tables
{
    public static class CultureCategoryDemandTable
    {
        public static List<EconomyDatabase.CultureCategoryDemandMultiplier> Read(
            Dictionary<string, CultureId> cultureMap,
            Dictionary<string, ItemType> itemTypeMap)
        {
            string csvPath = CsvPath("culture_category_demand_mult.csv");
            if (!File.Exists(csvPath))
            {
                Debug.LogWarning("[SPJ] culture_category_demand_mult.csv not found. Using default multipliers.");
                return BuildDefaults(cultureMap, itemTypeMap);
            }

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count <= 1)
            {
                Debug.LogWarning("[SPJ] culture_category_demand_mult.csv has no data rows. Using default multipliers.");
                return BuildDefaults(cultureMap, itemTypeMap);
            }

            string[] header = rows[0];
            int cultureIndex = FindColumnIndex(header, "culture_id");
            int categoryIndex = FindColumnIndex(header, "category_id");
            int multIndex = FindColumnIndex(header, "demand_mult");
            if (cultureIndex < 0 || categoryIndex < 0 || multIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in culture_category_demand_mult.csv");
                return new List<EconomyDatabase.CultureCategoryDemandMultiplier>();
            }

            List<EconomyDatabase.CultureCategoryDemandMultiplier> result = new();
            bool hadRow = false;
            for (int i = 1; i < rows.Count; i++)
            {
                string cultureId = GetField(rows[i], cultureIndex).Trim();
                if (string.IsNullOrWhiteSpace(cultureId))
                    continue;

                hadRow = true;
                if (!cultureMap.TryGetValue(cultureId, out CultureId culture))
                {
                    Debug.LogWarning($"[SPJ] Unknown culture_id '{cultureId}' in culture_category_demand_mult.csv (row {i + 1})");
                    continue;
                }

                string categoryId = GetField(rows[i], categoryIndex).Trim();
                if (!itemTypeMap.TryGetValue(categoryId, out ItemType category))
                {
                    Debug.LogWarning($"[SPJ] Unknown category_id '{categoryId}' in culture_category_demand_mult.csv (row {i + 1})");
                    continue;
                }

                if (!TryParseFloat(GetField(rows[i], multIndex), out float mult))
                {
                    Debug.LogWarning($"[SPJ] Invalid mult '{GetField(rows[i], multIndex)}' in culture_category_demand_mult.csv (row {i + 1})");
                    continue;
                }

                result.Add(new EconomyDatabase.CultureCategoryDemandMultiplier
                {
                    Culture = culture,
                    Category = category,
                    Multiplier = mult
                });
            }

            if (!hadRow || result.Count == 0)
            {
                Debug.LogWarning("[SPJ] culture_category_demand_mult.csv had no valid rows. Using default multipliers.");
                return BuildDefaults(cultureMap, itemTypeMap);
            }

            return result;
        }

        private static List<EconomyDatabase.CultureCategoryDemandMultiplier> BuildDefaults(
            Dictionary<string, CultureId> cultureMap,
            Dictionary<string, ItemType> itemTypeMap)
        {
            List<EconomyDatabase.CultureCategoryDemandMultiplier> result = new();
            List<CultureId> cultures = GetSortedCultureList(cultureMap);
            List<ItemType> categories = GetSortedCategoryList(itemTypeMap);

            foreach (CultureId culture in cultures)
            {
                foreach (ItemType category in categories)
                {
                    result.Add(new EconomyDatabase.CultureCategoryDemandMultiplier
                    {
                        Culture = culture,
                        Category = category,
                        Multiplier = 1f
                    });
                }
            }

            return result;
        }

        private static List<CultureId> GetSortedCultureList(Dictionary<string, CultureId> cultureMap)
        {
            List<string> keys = new List<string>(cultureMap.Keys);
            keys.Sort(StringComparer.Ordinal);

            List<CultureId> cultures = new List<CultureId>();
            foreach (string key in keys)
            {
                CultureId culture = cultureMap[key];
                if (culture == CultureId.None)
                    continue;
                if (!cultures.Contains(culture))
                    cultures.Add(culture);
            }

            return cultures;
        }

        private static List<ItemType> GetSortedCategoryList(Dictionary<string, ItemType> itemTypeMap)
        {
            List<string> keys = new List<string>(itemTypeMap.Keys);
            keys.Sort(StringComparer.Ordinal);

            List<ItemType> categories = new List<ItemType>();
            foreach (string key in keys)
            {
                ItemType category = itemTypeMap[key];
                if (category == ItemType.Unknown)
                    continue;
                if (!categories.Contains(category))
                    categories.Add(category);
            }

            return categories;
        }
    }
}

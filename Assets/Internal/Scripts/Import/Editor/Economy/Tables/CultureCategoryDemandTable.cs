using System;
using System.Collections.Generic;
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
            List<string[]> rows = CsvReader.ReadFileSafe(CsvPath("culture_category_demand_mult.csv"));
            if (rows == null || rows.Count <= 1)
            {
                Debug.LogWarning("[SPJ] culture_category_demand_mult.csv not found or empty. Using default multipliers.");
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
            List<CultureId> cultures = cultureMap.ToSortedUniqueValues(CultureId.None);
            List<ItemType> categories = itemTypeMap.ToSortedUniqueValues(ItemType.Unknown);

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

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy.Tables
{
    public static class CityTypeStockProfileTable
    {
        public static Dictionary<string, List<CityTypeData.CategoryStockProfile>> Read(
            Dictionary<string, ItemType> itemTypeMap)
        {
            Dictionary<string, List<CityTypeData.CategoryStockProfile>> result =
                new Dictionary<string, List<CityTypeData.CategoryStockProfile>>(StringComparer.Ordinal);

            string csvPath = CsvPath("city_type_category_stock_profile.csv");
            if (!File.Exists(csvPath))
            {
                Debug.LogWarning("[SPJ] city_type_category_stock_profile.csv not found. Stock profiles will be empty.");
                return result;
            }

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count <= 1)
            {
                Debug.LogWarning("[SPJ] city_type_category_stock_profile.csv has no data rows. Defaults will be used.");
                return result;
            }

            string[] header = rows[0];
            int cityTypeIndex = FindColumnIndex(header, "city_type_id");
            int categoryIndex = FindColumnIndex(header, "category_id");
            int desiredIndex = FindColumnIndex(header, "desired_per_scale");
            int dailyNetIndex = FindColumnIndex(header, "daily_net");
            int equilibriumIndex = FindColumnIndex(header, "equilibrium_pull");
            if (cityTypeIndex < 0 || categoryIndex < 0 || desiredIndex < 0 ||
                dailyNetIndex < 0 || equilibriumIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in city_type_category_stock_profile.csv");
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string cityTypeId = GetField(rows[i], cityTypeIndex).Trim();
                if (string.IsNullOrWhiteSpace(cityTypeId))
                    continue;

                string categoryId = GetField(rows[i], categoryIndex).Trim();
                if (!itemTypeMap.TryGetValue(categoryId, out ItemType category))
                {
                    Debug.LogWarning($"[SPJ] Unknown category_id '{categoryId}' in city_type_category_stock_profile.csv (row {i + 1})");
                    category = ItemType.Unknown;
                }

                TryParseFloat(GetField(rows[i], desiredIndex), out float desiredPerScale);
                TryParseFloat(GetField(rows[i], dailyNetIndex), out float dailyNet);
                TryParseFloat(GetField(rows[i], equilibriumIndex), out float equilibriumPull);

                if (!result.TryGetValue(cityTypeId, out List<CityTypeData.CategoryStockProfile> list))
                {
                    list = new List<CityTypeData.CategoryStockProfile>();
                    result[cityTypeId] = list;
                }

                list.Add(new CityTypeData.CategoryStockProfile
                {
                    Category = category,
                    DesiredPerScale = desiredPerScale,
                    DailyNet = dailyNet,
                    EquilibriumPull = equilibriumPull
                });
            }

            return result;
        }
    }
}

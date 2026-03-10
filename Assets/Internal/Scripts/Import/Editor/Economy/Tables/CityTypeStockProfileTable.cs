using System;
using System.Collections.Generic;
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

            List<string[]> rows = CsvReader.ReadFileSafe(CsvPath("city_type_category_stock_profile.csv"));
            if (rows == null || rows.Count <= 1)
            {
                Debug.LogWarning("[SPJ] city_type_category_stock_profile.csv not found or empty. Defaults will be used.");
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
                ItemType category = TryLookup(itemTypeMap, categoryId, ItemType.Unknown,
                    "city_type_category_stock_profile.csv", i + 1, "category_id");

                TryParseFloat(GetField(rows[i], desiredIndex), out float desiredPerScale);
                TryParseFloat(GetField(rows[i], dailyNetIndex), out float dailyNet);
                TryParseFloat(GetField(rows[i], equilibriumIndex), out float equilibriumPull);

                result.GetOrCreateList(cityTypeId).Add(new CityTypeData.CategoryStockProfile
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

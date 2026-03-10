using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy.Tables
{
    public static class CityTypeCoefsTable
    {
        public static Dictionary<string, List<CityTypeData.CategoryCoef>> Read(
            Dictionary<string, ItemType> itemTypeMap)
        {
            Dictionary<string, List<CityTypeData.CategoryCoef>> result =
                new Dictionary<string, List<CityTypeData.CategoryCoef>>(StringComparer.Ordinal);

            List<string[]> rows = CsvReader.ReadFileSafe(CsvPath("city_type_category_coefs.csv"));
            if (rows == null)
                return result;

            string[] header = rows[0];
            int cityTypeIndex = FindColumnIndex(header, "city_type_id");
            int categoryIndex = FindColumnIndex(header, "category_id");
            int buyIndex = FindColumnIndex(header, "buy_coef");
            int sellIndex = FindColumnIndex(header, "sell_coef");
            if (cityTypeIndex < 0 || categoryIndex < 0 || buyIndex < 0 || sellIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in city_type_category_coefs.csv");
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string cityTypeId = GetField(rows[i], cityTypeIndex).Trim();
                if (string.IsNullOrWhiteSpace(cityTypeId))
                    continue;

                string categoryId = GetField(rows[i], categoryIndex).Trim();
                ItemType category = TryLookup(itemTypeMap, categoryId, ItemType.Unknown,
                    "city_type_category_coefs.csv", i + 1, "category_id");

                if (!TryParseFloat(GetField(rows[i], buyIndex), out float buy))
                {
                    Debug.LogWarning($"[SPJ] Invalid buy_coef '{GetField(rows[i], buyIndex)}' (row {i + 1})");
                }

                if (!TryParseFloat(GetField(rows[i], sellIndex), out float sell))
                {
                    Debug.LogWarning($"[SPJ] Invalid sell_coef '{GetField(rows[i], sellIndex)}' (row {i + 1})");
                }

                result.GetOrCreateList(cityTypeId).Add(new CityTypeData.CategoryCoef
                {
                    Category = category,
                    BuyCoef = buy,
                    SellCoef = sell
                });
            }

            return result;
        }
    }
}

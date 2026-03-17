using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Caravan.Tables
{
    public static class ExtraCartsTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/Caravan";
        private const string LOC_TABLE = "Caravan";

        public static List<ExtraCartData> Import(Dictionary<string, ExtraCartType> typeMap)
        {
            List<string[]> rows = CsvReader.ReadFile(CsvPath("extra_carts.csv"));
            List<ExtraCartData> result = new();

            if (rows.Count == 0)
                return result;

            string[] header = rows[0];
            int idIdx = FindColumnIndex(header, "extra_cart_id");
            int capIdx = FindColumnIndex(header, "capacity");
            int durIdx = FindColumnIndex(header, "durability");
            int speedPenIdx = FindColumnIndex(header, "speed_penalty_pct");
            int suppliesIdx = FindColumnIndex(header, "supplies_per_day");
            int priceIdx = FindColumnIndex(header, "price");
            int sellIdx = FindColumnIndex(header, "sell_price");
            int locIdx = FindColumnIndex(header, "loc_link");
            if (idIdx < 0 || capIdx < 0 || durIdx < 0 || speedPenIdx < 0 ||
                suppliesIdx < 0 || priceIdx < 0 || sellIdx < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in extra_carts.csv");
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIdx).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                ExtraCartType type = TryLookup(typeMap, id, ExtraCartType.Unknown, "extra_carts.csv", i + 1, "extra_cart_id");

                TryParseFloat(GetField(rows[i], capIdx), out float capacity);
                TryParseFloat(GetField(rows[i], durIdx), out float durability);
                TryParseFloat(GetField(rows[i], speedPenIdx), out float speedPenalty);
                TryParseInt(GetField(rows[i], suppliesIdx), out int supplies);
                TryParseInt(GetField(rows[i], priceIdx), out int price);
                TryParseInt(GetField(rows[i], sellIdx), out int sellPrice);

                ExtraCartData asset = LoadOrCreateAsset<ExtraCartData>(OUTPUT_FOLDER, id);
                asset.ApplyImport(id, type, capacity, durability, speedPenalty, supplies, price, sellPrice,
                    MakeLocalizedString(GetField(rows[i], locIdx).Trim(), LOC_TABLE));

                EditorUtility.SetDirty(asset);
                result.Add(asset);
            }

            return result;
        }
    }
}

using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Import.Editor.Core;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy.Tables
{
    public static class ItemsTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/Items";

        public static List<ItemData> Import(
            Dictionary<string, ItemType> typeMap,
            string locTableName)
        {
            string csvPath = CsvPath("items.csv");
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            List<ItemData> items = new();

            if (rows.Count == 0)
                return items;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "item_id");
            int nameIndex = FindColumnIndex(header, "name_key");
            int categoryIndex = FindColumnIndex(header, "category_id");
            int weightIndex = FindColumnIndex(header, "weight_kg");
            int priceIndex = FindColumnIndex(header, "base_price");
            int demandWeightIndex = FindColumnIndex(header, "demand_weight");
            if (idIndex < 0 || nameIndex < 0 || categoryIndex < 0 ||
                weightIndex < 0 || priceIndex < 0 || demandWeightIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in items.csv (expected demand_weight)");
                return items;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                string categoryId = GetField(rows[i], categoryIndex).Trim();
                ItemType type = TryLookup(typeMap, categoryId, ItemType.Unknown, "items.csv", i + 1, "category_id");

                TryParseFloat(GetField(rows[i], weightIndex), out float weight);
                TryParseInt(GetField(rows[i], priceIndex), out int price);
                TryParseFloat(GetField(rows[i], demandWeightIndex), out float demandWeight);

                ItemData asset = LoadOrCreateAsset<ItemData>(OUTPUT_FOLDER, id);

                asset.ApplyImport(
                    id,
                    type,
                    weight,
                    price,
                    demandWeight,
                    MakeLocalizedString(GetField(rows[i], nameIndex).Trim(), locTableName));

                EditorUtility.SetDirty(asset);
                items.Add(asset);
            }

            return items;
        }

        public static HashSet<string> ReadIds()
        {
            HashSet<string> ids = new(System.StringComparer.Ordinal);
            List<string[]> rows = CsvReader.ReadFileSafe(CsvPath("items.csv"));
            if (rows == null)
                return ids;

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "item_id");
            if (idIndex < 0)
                return ids;

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;
                ids.Add(id);
            }

            return ids;
        }
    }
}

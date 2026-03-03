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
    public static class CultureItemDemandTable
    {
        public static List<EconomyDatabase.CultureItemDemandMultiplier> Read(
            Dictionary<string, CultureId> cultureMap,
            HashSet<string> itemIds)
        {
            string csvPath = CsvPath("culture_item_demand_mult.csv");
            if (!File.Exists(csvPath))
            {
                Debug.LogWarning("[SPJ] culture_item_demand_mult.csv not found. Using default multipliers.");
                return BuildDefaults(cultureMap, itemIds);
            }

            List<string[]> rows = CsvReader.ReadFile(csvPath);
            if (rows.Count <= 1)
            {
                Debug.LogWarning("[SPJ] culture_item_demand_mult.csv has no data rows. Using default multipliers.");
                return BuildDefaults(cultureMap, itemIds);
            }

            string[] header = rows[0];
            int cultureIndex = FindColumnIndex(header, "culture_id");
            int itemIndex = FindColumnIndex(header, "item_id");
            int multIndex = FindColumnIndex(header, "demand_mult");
            if (cultureIndex < 0 || itemIndex < 0 || multIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in culture_item_demand_mult.csv");
                return new List<EconomyDatabase.CultureItemDemandMultiplier>();
            }

            List<EconomyDatabase.CultureItemDemandMultiplier> result = new();
            bool hadRow = false;
            for (int i = 1; i < rows.Count; i++)
            {
                string cultureId = GetField(rows[i], cultureIndex).Trim();
                if (string.IsNullOrWhiteSpace(cultureId))
                    continue;

                hadRow = true;
                if (!cultureMap.TryGetValue(cultureId, out CultureId culture))
                {
                    Debug.LogWarning($"[SPJ] Unknown culture_id '{cultureId}' in culture_item_demand_mult.csv (row {i + 1})");
                    continue;
                }

                string itemId = GetField(rows[i], itemIndex).Trim();
                if (string.IsNullOrWhiteSpace(itemId))
                    continue;

                if (itemIds.Count > 0 && !itemIds.Contains(itemId))
                {
                    Debug.LogWarning($"[SPJ] Unknown item_id '{itemId}' in culture_item_demand_mult.csv (row {i + 1})");
                    continue;
                }

                if (!TryParseFloat(GetField(rows[i], multIndex), out float mult))
                {
                    Debug.LogWarning($"[SPJ] Invalid mult '{GetField(rows[i], multIndex)}' in culture_item_demand_mult.csv (row {i + 1})");
                    continue;
                }

                result.Add(new EconomyDatabase.CultureItemDemandMultiplier
                {
                    Culture = culture,
                    ItemId = itemId,
                    Multiplier = mult
                });
            }

            if (!hadRow || result.Count == 0)
            {
                Debug.LogWarning("[SPJ] culture_item_demand_mult.csv had no valid rows. Using default multipliers.");
                return BuildDefaults(cultureMap, itemIds);
            }

            return result;
        }

        private static List<EconomyDatabase.CultureItemDemandMultiplier> BuildDefaults(
            Dictionary<string, CultureId> cultureMap,
            HashSet<string> itemIds)
        {
            List<EconomyDatabase.CultureItemDemandMultiplier> result = new();
            List<CultureId> cultures = cultureMap.ToSortedUniqueValues(CultureId.None);
            List<string> items = new List<string>(itemIds);
            items.Sort(StringComparer.Ordinal);

            foreach (CultureId culture in cultures)
            {
                foreach (string itemId in items)
                {
                    result.Add(new EconomyDatabase.CultureItemDemandMultiplier
                    {
                        Culture = culture,
                        ItemId = itemId,
                        Multiplier = 1f
                    });
                }
            }

            return result;
        }

    }
}

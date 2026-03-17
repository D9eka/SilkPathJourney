using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Caravan.Tables
{
    public static class CartUpgradeLevelsTable
    {
        private const string LOC_TABLE = "Caravan";

        public static List<CaravanDatabase.CartUpgradeLevelEntry> Read(
            Dictionary<string, CartUpgradeLevel> levelMap)
        {
            List<string[]> rows = CsvReader.ReadFileSafe(CsvPath("cart_upgrade_levels.csv"));
            if (rows == null)
                return new List<CaravanDatabase.CartUpgradeLevelEntry>();

            string[] header = rows[0];
            int idIdx = FindColumnIndex(header, "level_id");
            int sortIdx = FindColumnIndex(header, "sort_order");
            int speedIdx = FindColumnIndex(header, "speed_mult");
            int capIdx = FindColumnIndex(header, "capacity_mult");
            int durIdx = FindColumnIndex(header, "durability_mult");
            int priceIdx = FindColumnIndex(header, "price");
            int locIdx = FindColumnIndex(header, "loc_link");
            if (idIdx < 0 || sortIdx < 0 || speedIdx < 0 || capIdx < 0 || durIdx < 0 || priceIdx < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in cart_upgrade_levels.csv");
                return new List<CaravanDatabase.CartUpgradeLevelEntry>();
            }

            List<CaravanDatabase.CartUpgradeLevelEntry> result = new();
            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIdx).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                CartUpgradeLevel level = TryLookup(levelMap, id, CartUpgradeLevel.Unknown,
                    "cart_upgrade_levels.csv", i + 1, "level_id");

                TryParseInt(GetField(rows[i], sortIdx), out int sortOrder);
                TryParseFloat(GetField(rows[i], speedIdx), out float speedMult);
                TryParseFloat(GetField(rows[i], capIdx), out float capMult);
                TryParseFloat(GetField(rows[i], durIdx), out float durMult);
                TryParseInt(GetField(rows[i], priceIdx), out int price);

                result.Add(new CaravanDatabase.CartUpgradeLevelEntry
                {
                    Level = level,
                    SortOrder = sortOrder,
                    SpeedMult = speedMult,
                    CapacityMult = capMult,
                    DurabilityMult = durMult,
                    Price = price,
                    Name = MakeLocalizedString(GetField(rows[i], locIdx).Trim(), LOC_TABLE)
                });
            }

            return result;
        }
    }
}

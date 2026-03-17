using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Caravan.Tables
{
    public static class CompanionQualityLevelsTable
    {
        private const string LOC_TABLE = "Caravan";

        public static List<CaravanDatabase.CompanionQualityEntry> Read(
            Dictionary<string, CompanionQuality> qualityMap)
        {
            List<string[]> rows = CsvReader.ReadFileSafe(CsvPath("companion_quality_levels.csv"));
            if (rows == null)
                return new List<CaravanDatabase.CompanionQualityEntry>();

            string[] header = rows[0];
            int idIdx = FindColumnIndex(header, "quality_id");
            int sortIdx = FindColumnIndex(header, "sort_order");
            int successIdx = FindColumnIndex(header, "success_pct");
            int priceMultIdx = FindColumnIndex(header, "price_multiplier");
            int dailyCostIdx = FindColumnIndex(header, "daily_cost_multiplier");
            int availIdx = FindColumnIndex(header, "availability");
            int locIdx = FindColumnIndex(header, "loc_link");
            if (idIdx < 0 || sortIdx < 0 || successIdx < 0 || priceMultIdx < 0 ||
                dailyCostIdx < 0 || availIdx < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in companion_quality_levels.csv");
                return new List<CaravanDatabase.CompanionQualityEntry>();
            }

            List<CaravanDatabase.CompanionQualityEntry> result = new();
            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIdx).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                CompanionQuality quality = TryLookup(qualityMap, id, CompanionQuality.Unknown,
                    "companion_quality_levels.csv", i + 1, "quality_id");

                TryParseInt(GetField(rows[i], sortIdx), out int sortOrder);
                TryParseFloat(GetField(rows[i], successIdx), out float successPct);
                TryParseFloat(GetField(rows[i], priceMultIdx), out float priceMult);
                TryParseFloat(GetField(rows[i], dailyCostIdx), out float dailyCostMult);
                string availability = GetField(rows[i], availIdx).Trim();

                result.Add(new CaravanDatabase.CompanionQualityEntry
                {
                    Quality = quality,
                    SortOrder = sortOrder,
                    SuccessPct = successPct,
                    PriceMultiplier = priceMult,
                    DailyCostMultiplier = dailyCostMult,
                    Availability = availability,
                    Name = MakeLocalizedString(GetField(rows[i], locIdx).Trim(), LOC_TABLE)
                });
            }

            return result;
        }
    }
}

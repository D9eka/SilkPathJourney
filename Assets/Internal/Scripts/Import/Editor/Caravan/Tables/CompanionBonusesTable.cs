using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Caravan.Tables
{
    public static class CompanionBonusesTable
    {
        private const string LOC_TABLE = "Caravan";

        public static List<CaravanDatabase.CompanionBonusEntry> Read(
            Dictionary<string, CompanionType> typeMap,
            Dictionary<string, CompanionQuality> qualityMap)
        {
            List<string[]> rows = CsvReader.ReadFileSafe(CsvPath("companion_bonuses.csv"));
            if (rows == null)
                return new List<CaravanDatabase.CompanionBonusEntry>();

            string[] header = rows[0];
            int typeIdx = FindColumnIndex(header, "type_id");
            int qualityIdx = FindColumnIndex(header, "quality_id");
            int keyIdx = FindColumnIndex(header, "bonus_key");
            int valueIdx = FindColumnIndex(header, "bonus_value");
            int locIdx = FindColumnIndex(header, "loc_link");
            if (typeIdx < 0 || qualityIdx < 0 || keyIdx < 0 || valueIdx < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in companion_bonuses.csv");
                return new List<CaravanDatabase.CompanionBonusEntry>();
            }

            List<CaravanDatabase.CompanionBonusEntry> result = new();
            for (int i = 1; i < rows.Count; i++)
            {
                string typeId = GetField(rows[i], typeIdx).Trim();
                if (string.IsNullOrWhiteSpace(typeId))
                    continue;

                CompanionType type = TryLookup(typeMap, typeId, CompanionType.Unknown,
                    "companion_bonuses.csv", i + 1, "type_id");

                string qualityId = GetField(rows[i], qualityIdx).Trim();
                CompanionQuality quality = TryLookup(qualityMap, qualityId, CompanionQuality.Unknown,
                    "companion_bonuses.csv", i + 1, "quality_id");

                string bonusKey = GetField(rows[i], keyIdx).Trim();
                TryParseFloat(GetField(rows[i], valueIdx), out float bonusValue);

                result.Add(new CaravanDatabase.CompanionBonusEntry
                {
                    Type = type,
                    Quality = quality,
                    BonusKey = bonusKey,
                    BonusValue = bonusValue,
                    Description = MakeLocalizedString(GetField(rows[i], locIdx).Trim(), LOC_TABLE)
                });
            }

            return result;
        }
    }
}

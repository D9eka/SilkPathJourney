using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Caravan.Tables
{
    public static class DraftAnimalsTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/Caravan";
        private const string LOC_TABLE = "Caravan";

        public static List<DraftAnimalData> Import(Dictionary<string, DraftAnimalType> typeMap)
        {
            List<string[]> rows = CsvReader.ReadFile(CsvPath("draft_animals.csv"));
            List<DraftAnimalData> result = new();

            if (rows.Count == 0)
                return result;

            string[] header = rows[0];
            int idIdx = FindColumnIndex(header, "animal_id");
            int speedIdx = FindColumnIndex(header, "speed_mod_pct");
            int capIdx = FindColumnIndex(header, "capacity_mod_pct");
            int feedIdx = FindColumnIndex(header, "feed_per_day");
            int priceIdx = FindColumnIndex(header, "price");
            int regionIdx = FindColumnIndex(header, "buy_region");
            int biomeIdx = FindColumnIndex(header, "suitable_biome_bonus");
            int locIdx = FindColumnIndex(header, "loc_link");
            if (idIdx < 0 || speedIdx < 0 || capIdx < 0 || feedIdx < 0 || priceIdx < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in draft_animals.csv");
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIdx).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                DraftAnimalType animalType = TryLookup(typeMap, id, DraftAnimalType.Unknown, "draft_animals.csv", i + 1, "animal_id");

                TryParseFloat(GetField(rows[i], speedIdx), out float speedMod);
                TryParseFloat(GetField(rows[i], capIdx), out float capMod);
                TryParseFloat(GetField(rows[i], feedIdx), out float feed);
                TryParseInt(GetField(rows[i], priceIdx), out int price);
                string buyRegion = GetField(rows[i], regionIdx).Trim();
                string suitableBiomeBonus = GetField(rows[i], biomeIdx).Trim();

                DraftAnimalData asset = LoadOrCreateAsset<DraftAnimalData>(OUTPUT_FOLDER, id);
                asset.ApplyImport(id, animalType, speedMod, capMod, feed, price, buyRegion, suitableBiomeBonus,
                    MakeLocalizedString(GetField(rows[i], locIdx).Trim(), LOC_TABLE));

                EditorUtility.SetDirty(asset);
                result.Add(asset);
            }

            return result;
        }
    }
}

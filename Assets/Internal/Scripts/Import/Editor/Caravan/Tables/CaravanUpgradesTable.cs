using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Caravan.Tables
{
    public static class CaravanUpgradesTable
    {
        private const string LOC_TABLE = "Caravan";

        public static List<CaravanDatabase.CaravanUpgradeEntry> Read(
            Dictionary<string, CaravanUpgradeType> upgradeMap,
            Dictionary<string, BuildingId> buildingMap)
        {
            List<string[]> rows = CsvReader.ReadFileSafe(CsvPath("caravan_upgrades.csv"));
            if (rows == null)
                return new List<CaravanDatabase.CaravanUpgradeEntry>();

            string[] header = rows[0];
            int idIdx = FindColumnIndex(header, "upgrade_id");
            int priceIdx = FindColumnIndex(header, "price");
            int buildingIdx = FindColumnIndex(header, "building_required");
            int effectIdx = FindColumnIndex(header, "effect_description");
            int locIdx = FindColumnIndex(header, "loc_link");
            int descLocIdx = FindColumnIndex(header, "desc_loc_link");
            if (idIdx < 0 || priceIdx < 0 || buildingIdx < 0 || effectIdx < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in caravan_upgrades.csv");
                return new List<CaravanDatabase.CaravanUpgradeEntry>();
            }

            List<CaravanDatabase.CaravanUpgradeEntry> result = new();
            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIdx).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                CaravanUpgradeType type = TryLookup(upgradeMap, id, CaravanUpgradeType.Unknown,
                    "caravan_upgrades.csv", i + 1, "upgrade_id");

                TryParseInt(GetField(rows[i], priceIdx), out int price);

                string buildingId = GetField(rows[i], buildingIdx).Trim();
                BuildingId building = TryLookup(buildingMap, buildingId, BuildingId.Unknown,
                    "caravan_upgrades.csv", i + 1, "building_required");

                string effectDesc = GetField(rows[i], effectIdx).Trim();
                string descLocLink = descLocIdx >= 0 ? GetField(rows[i], descLocIdx).Trim() : "";

                result.Add(new CaravanDatabase.CaravanUpgradeEntry
                {
                    Type = type,
                    Price = price,
                    Building = building,
                    EffectDescription = effectDesc,
                    Name = MakeLocalizedString(GetField(rows[i], locIdx).Trim(), LOC_TABLE),
                    Description = string.IsNullOrEmpty(descLocLink) ? null : MakeLocalizedString(descLocLink, LOC_TABLE)
                });
            }

            return result;
        }
    }
}

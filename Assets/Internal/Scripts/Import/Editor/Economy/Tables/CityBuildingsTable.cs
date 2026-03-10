using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy.Tables
{
    public static class CityBuildingsTable
    {
        public static Dictionary<string, List<BuildingId>> Read(Dictionary<string, BuildingId> buildingMap)
        {
            string csvPath = CsvPath("city_buildings.csv");
            var rows = CsvReader.ReadFile(csvPath);
            var result = new Dictionary<string, List<BuildingId>>();

            if (rows.Count == 0) return result;

            string[] header = rows[0];
            int cityIndex = FindColumnIndex(header, "city_id");
            int buildingIndex = FindColumnIndex(header, "building_id");

            if (cityIndex < 0 || buildingIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in city_buildings.csv");
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string cityId = GetField(rows[i], cityIndex).Trim();
                string buildingId = GetField(rows[i], buildingIndex).Trim();
                if (string.IsNullOrWhiteSpace(cityId) || string.IsNullOrWhiteSpace(buildingId))
                    continue;

                if (!buildingMap.TryGetValue(buildingId, out BuildingId bid))
                {
                    Debug.LogWarning($"[SPJ] Unknown building_id '{buildingId}' in city_buildings.csv (row {i + 1})");
                    continue;
                }

                if (!result.TryGetValue(cityId, out var list))
                    result[cityId] = list = new List<BuildingId>();

                list.Add(bid);
            }

            return result;
        }
    }
}

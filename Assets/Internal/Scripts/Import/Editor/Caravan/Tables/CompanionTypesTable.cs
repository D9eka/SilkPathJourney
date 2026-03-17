using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Caravan.Tables
{
    public static class CompanionTypesTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/Caravan";
        private const string LOC_TABLE = "Caravan";

        public static List<CompanionTypeData> Import(Dictionary<string, CompanionType> typeMap)
        {
            List<string[]> rows = CsvReader.ReadFile(CsvPath("companion_types.csv"));
            List<CompanionTypeData> result = new();

            if (rows.Count == 0)
                return result;

            string[] header = rows[0];
            int idIdx = FindColumnIndex(header, "type_id");
            int hireCostIdx = FindColumnIndex(header, "hire_cost_base");
            int dailyCostIdx = FindColumnIndex(header, "daily_cost_base");
            int locIdx = FindColumnIndex(header, "loc_link");
            if (idIdx < 0 || hireCostIdx < 0 || dailyCostIdx < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in companion_types.csv");
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIdx).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                CompanionType type = TryLookup(typeMap, id, CompanionType.Unknown, "companion_types.csv", i + 1, "type_id");

                TryParseInt(GetField(rows[i], hireCostIdx), out int hireCost);
                TryParseInt(GetField(rows[i], dailyCostIdx), out int dailyCost);

                CompanionTypeData asset = LoadOrCreateAsset<CompanionTypeData>(OUTPUT_FOLDER, id);
                asset.ApplyImport(id, type, hireCost, dailyCost,
                    MakeLocalizedString(GetField(rows[i], locIdx).Trim(), LOC_TABLE));

                EditorUtility.SetDirty(asset);
                result.Add(asset);
            }

            return result;
        }
    }
}

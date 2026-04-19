using System;
using System.Collections.Generic;
using Internal.Scripts.Camp;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Camp.Tables
{
    public static class CampBiomeModifiersTable
    {
        public static List<BiomeModifier> Read(
            Dictionary<string, Biome> biomeMap,
            string csvFile = "camp_biome_modifiers.csv")
        {
            List<BiomeModifier> result = new();

            var rows = CsvReader.ReadFileSafe(CsvPath(csvFile));
            if (rows == null || rows.Count <= 1)
                return result;

            string[] header = rows[0];
            int biomeIndex = FindColumnIndex(header, "biome");
            int actionTypeIndex = FindColumnIndex(header, "action_type");
            int modifierIndex = FindColumnIndex(header, "modifier");

            if (biomeIndex < 0 || actionTypeIndex < 0 || modifierIndex < 0)
            {
                Debug.LogError("[SPJ] Missing columns in " + csvFile);
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string biomeStr = GetField(rows[i], biomeIndex).Trim();
                string actionTypeStr = GetField(rows[i], actionTypeIndex).Trim();
                if (string.IsNullOrWhiteSpace(biomeStr) || string.IsNullOrWhiteSpace(actionTypeStr))
                    continue;

                Biome biome = TryLookup(biomeMap, biomeStr, Biome.Unknown, csvFile, i + 1, "biome");
                if (biome == Biome.Unknown)
                    continue;

                if (!Enum.TryParse(actionTypeStr, out CampActionType actionType))
                {
                    Debug.LogWarning($"[SPJ] Unknown action_type '{actionTypeStr}' in {csvFile} (row {i + 1})");
                    continue;
                }

                TryParseFloat(GetField(rows[i], modifierIndex), out float modifier);

                result.Add(new BiomeModifier
                {
                    Biome = biome,
                    ActionType = actionType,
                    Modifier = modifier
                });
            }

            return result;
        }
    }
}

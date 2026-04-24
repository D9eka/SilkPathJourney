using System;
using System.Collections.Generic;
using Internal.Scripts.Camp;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.Import.Editor.Core;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Camp.Tables
{
    public static class CampActionsTable
    {
        private const string OUTPUT_FOLDER = "Assets/Internal/Data/Camp";
        private const string LOC_TABLE = "UI";
        private const string CSV_FILE = "camp_actions.csv";

        public static List<CampActionData> Import(
            Dictionary<string, CampActionType> typeMap,
            Dictionary<string, List<RepeatSideEffect>> sideEffectsMap)
        {
            List<string[]> rows = CsvReader.ReadFile(CsvPath(CSV_FILE));
            List<CampActionData> result = new();

            if (rows.Count == 0)
                return result;

            string[] header = rows[0];
            int idIdx = FindColumnIndex(header, "action_id");
            int costIdx = FindColumnIndex(header, "cost_supplies");
            int resourceIdx = FindColumnIndex(header, "affected_resource");
            int skillIdx = FindColumnIndex(header, "related_skill");
            int maxRepeatIdx = FindColumnIndex(header, "max_repeat");
            int skipFoodIdx = FindColumnIndex(header, "skip_food");
            int curveIdx = FindColumnIndex(header, "diminishing_curve");
            int locNameIdx = FindColumnIndex(header, "loc_name");
            int locHint1Idx = FindColumnIndex(header, "loc_hint1");
            int locHint2Idx = FindColumnIndex(header, "loc_hint2");
            int locHint3Idx = FindColumnIndex(header, "loc_hint3plus");

            if (idIdx < 0 || costIdx < 0 || resourceIdx < 0 || skillIdx < 0 || maxRepeatIdx < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in " + CSV_FILE);
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIdx).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                CampActionType type = TryLookup(typeMap, id, CampActionType.Forage, CSV_FILE, i + 1, "action_id");

                TryParseFloat(GetField(rows[i], costIdx), out float costSupplies);

                string resourceStr = GetField(rows[i], resourceIdx).Trim();
                if (!Enum.TryParse(resourceStr, out EventOutcomeType affectedResource))
                {
                    Debug.LogWarning($"[SPJ] Unknown affected_resource '{resourceStr}' in {CSV_FILE} (row {i + 1})");
                    affectedResource = EventOutcomeType.None;
                }

                string skillStr = GetField(rows[i], skillIdx).Trim();
                if (!Enum.TryParse(skillStr, out SkillType relatedSkill))
                {
                    Debug.LogWarning($"[SPJ] Unknown related_skill '{skillStr}' in {CSV_FILE} (row {i + 1})");
                    relatedSkill = SkillType.None;
                }

                TryParseInt(GetField(rows[i], maxRepeatIdx), out int maxRepeat);
                bool skipFood = skipFoodIdx >= 0 && ParseBool(GetField(rows[i], skipFoodIdx));

                float[] diminishingCurve = ParseDiminishingCurve(GetField(rows[i], curveIdx));

                sideEffectsMap.TryGetValue(id, out var sideEffects);

                string locName = locNameIdx >= 0 ? GetField(rows[i], locNameIdx).Trim() : string.Empty;
                string locHint1 = locHint1Idx >= 0 ? GetField(rows[i], locHint1Idx).Trim() : string.Empty;
                string locHint2 = locHint2Idx >= 0 ? GetField(rows[i], locHint2Idx).Trim() : string.Empty;
                string locHint3 = locHint3Idx >= 0 ? GetField(rows[i], locHint3Idx).Trim() : string.Empty;

                string assetPath = $"{OUTPUT_FOLDER}/CampAction_{ToPascalCase(id)}.asset";
                CampActionData asset = LoadOrCreateAsset<CampActionData>(assetPath);

                asset.ApplyImport(
                    type, costSupplies, affectedResource,
                    relatedSkill, maxRepeat, diminishingCurve,
                    skipFood, sideEffects,
                    MakeLocalizedString(locName, LOC_TABLE),
                    MakeLocalizedString(locHint1, LOC_TABLE),
                    MakeLocalizedString(locHint2, LOC_TABLE),
                    MakeLocalizedString(locHint3, LOC_TABLE));

                EditorUtility.SetDirty(asset);
                result.Add(asset);
            }

            return result;
        }

        private static float[] ParseDiminishingCurve(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return Array.Empty<float>();

            string[] parts = raw.Split(';');
            List<float> values = new(parts.Length);
            foreach (string part in parts)
            {
                if (TryParseFloat(part.Trim(), out float f))
                    values.Add(f);
            }
            return values.ToArray();
        }
    }
}

using System;
using System.Collections.Generic;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Meta.Achievements;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Achievement
{
    public static class AchievementsTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/Achievements";
        private const string LOC_TABLE = "Player";

        public static List<AchievementData> Import()
        {
            List<string[]> rows = CsvReader.ReadFile(CsvPath("achievements.csv"));
            List<AchievementData> result = new();

            if (rows.Count == 0)
                return result;

            string[] header = rows[0];
            int idIdx = FindColumnIndex(header, "achievement_id");
            int nameKeyIdx = FindColumnIndex(header, "name_key");
            int descKeyIdx = FindColumnIndex(header, "description_key");
            int triggerIdx = FindColumnIndex(header, "trigger");
            int valueIdx = FindColumnIndex(header, "value");
            int rewardIdx = FindColumnIndex(header, "legacy_reward");
            int iconIdx = FindColumnIndex(header, "icon");

            if (idIdx < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in achievements.csv");
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIdx).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                string triggerRaw = GetField(rows[i], triggerIdx).Trim();
                if (!Enum.TryParse(triggerRaw, true, out AchievementTrigger trigger))
                {
                    Debug.LogWarning($"[SPJ] Unknown AchievementTrigger '{triggerRaw}' in achievements.csv (row {i + 1})");
                    trigger = AchievementTrigger.MoneyEarned;
                }

                TryParseInt(GetField(rows[i], valueIdx), out int value);
                TryParseInt(GetField(rows[i], rewardIdx), out int legacyReward);

                var nameKey = MakeLocalizedString(GetField(rows[i], nameKeyIdx).Trim(), LOC_TABLE);
                var descKey = MakeLocalizedString(GetField(rows[i], descKeyIdx).Trim(), LOC_TABLE);
                var icon = LoadSprite(OUTPUT_FOLDER, rows[i], iconIdx, id);

                AchievementData asset = LoadOrCreateAsset<AchievementData>(OUTPUT_FOLDER, id);
                asset.ApplyImport(id, nameKey, descKey, trigger, value, legacyReward, icon);
                EditorUtility.SetDirty(asset);
                result.Add(asset);
            }

            return result;
        }
    }
}

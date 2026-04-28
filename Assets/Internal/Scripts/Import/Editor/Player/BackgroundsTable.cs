using System;
using System.Collections.Generic;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Player.Background;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.Player.Skills;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Player
{
    public static class BackgroundsTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/Backgrounds";
        private const string LOC_TABLE = "Player";

        public static List<BackgroundData> Import()
        {
            List<string[]> rows = CsvReader.ReadFile(CsvPath("backgrounds.csv"));
            List<BackgroundData> result = new();

            if (rows.Count == 0)
                return result;

            string[] header = rows[0];
            int idIdx = FindColumnIndex(header, "background_id");
            int locIdx = FindColumnIndex(header, "loc_link");
            int descLocIdx = FindColumnIndex(header, "desc_loc_link");
            int isDefaultIdx = FindColumnIndex(header, "is_default");
            int unlockCostIdx = FindColumnIndex(header, "unlock_cost");
            int tradeIdx = FindColumnIndex(header, "skill_trade");
            int charismaIdx = FindColumnIndex(header, "skill_charisma");
            int survivalIdx = FindColumnIndex(header, "skill_survival");
            int roleKeyIdx = FindColumnIndex(header, "role_key");
            int difficultyIdx = FindColumnIndex(header, "difficulty");
            int mainSkillIdx = FindColumnIndex(header, "main_skill");
            int secondarySkillIdx = FindColumnIndex(header, "secondary_skill");
            int featuresKeyIdx = FindColumnIndex(header, "features_key");
            int isRecommendedIdx = FindColumnIndex(header, "is_recommended");
            int moneyIdx = FindColumnIndex(header, "starting_money");
            int suppliesIdx = FindColumnIndex(header, "starting_supplies");
            int reputationIdx = FindColumnIndex(header, "starting_reputation");
            int moraleBonusIdx = FindColumnIndex(header, "starting_morale_bonus");
            int languagesIdx = FindColumnIndex(header, "starting_languages");
            int itemsIdx = FindColumnIndex(header, "starting_items");
            int startingCityIdx = FindColumnIndex(header, "starting_city_id");

            if (idIdx < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in backgrounds.csv");
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIdx).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                TryParseInt(GetField(rows[i], unlockCostIdx), out int unlockCost);
                TryParseInt(GetField(rows[i], tradeIdx), out int trade);
                TryParseInt(GetField(rows[i], charismaIdx), out int charisma);
                TryParseInt(GetField(rows[i], survivalIdx), out int survival);
                TryParseInt(GetField(rows[i], moneyIdx), out int money);
                TryParseInt(GetField(rows[i], suppliesIdx), out int supplies);
                TryParseInt(GetField(rows[i], reputationIdx), out int reputation);
                TryParseInt(GetField(rows[i], moraleBonusIdx), out int moraleBonus);

                bool isDefault = ParseBool(GetField(rows[i], isDefaultIdx));
                bool isRecommended = ParseBool(GetField(rows[i], isRecommendedIdx));

                string roleKey = GetField(rows[i], roleKeyIdx).Trim();
                string featuresKey = GetField(rows[i], featuresKeyIdx).Trim();

                BackgroundDifficulty difficulty = ParseDifficulty(GetField(rows[i], difficultyIdx));
                SkillType mainSkill = ParseSkillType(GetField(rows[i], mainSkillIdx));
                SkillType secondarySkill = ParseSkillType(GetField(rows[i], secondarySkillIdx));

                var languages = ParseLanguages(GetField(rows[i], languagesIdx));
                var items = ParseItems(GetField(rows[i], itemsIdx));
                string startingCityId = startingCityIdx >= 0 ? GetField(rows[i], startingCityIdx).Trim() : string.Empty;

                BackgroundData asset = LoadOrCreateAsset<BackgroundData>(OUTPUT_FOLDER, id);
                asset.ApplyImport(
                    id,
                    MakeLocalizedString(GetField(rows[i], locIdx).Trim(), LOC_TABLE),
                    MakeLocalizedString(GetField(rows[i], descLocIdx).Trim(), LOC_TABLE),
                    isDefault,
                    unlockCost,
                    trade,
                    charisma,
                    survival,
                    roleKey,
                    difficulty,
                    mainSkill,
                    secondarySkill,
                    featuresKey,
                    isRecommended,
                    languages,
                    money,
                    supplies,
                    reputation,
                    moraleBonus,
                    items,
                    startingCityId);

                EditorUtility.SetDirty(asset);
                result.Add(asset);
            }

            return result;
        }

        private static BackgroundDifficulty ParseDifficulty(string raw)
        {
            if (Enum.TryParse(raw.Trim(), true, out BackgroundDifficulty result))
                return result;
            return BackgroundDifficulty.Normal;
        }

        private static SkillType ParseSkillType(string raw)
        {
            string trimmed = raw.Trim();
            if (string.IsNullOrEmpty(trimmed))
                return SkillType.None;
            if (Enum.TryParse(trimmed, true, out SkillType result))
                return result;
            Debug.LogWarning($"[SPJ] Unknown SkillType '{trimmed}' in backgrounds.csv");
            return SkillType.None;
        }

        private static List<LanguageStartEntry> ParseLanguages(string raw)
        {
            var result = new List<LanguageStartEntry>();
            if (string.IsNullOrWhiteSpace(raw))
                return result;

            foreach (string part in raw.Split('|'))
            {
                string[] kv = part.Trim().Split(':');
                if (kv.Length != 2)
                    continue;

                string langName = kv[0].Trim();
                if (!Enum.TryParse(langName, out LanguageType langType))
                {
                    Debug.LogWarning($"[SPJ] Unknown LanguageType '{langName}' in backgrounds.csv");
                    continue;
                }

                TryParseInt(kv[1].Trim(), out int level);
                result.Add(new LanguageStartEntry(langType, level));
            }

            return result;
        }

        private static List<ItemStartEntry> ParseItems(string raw)
        {
            var result = new List<ItemStartEntry>();
            if (string.IsNullOrWhiteSpace(raw))
                return result;

            foreach (string part in raw.Split('|'))
            {
                string[] kv = part.Trim().Split(':');
                if (kv.Length != 2)
                    continue;

                string itemId = kv[0].Trim();
                TryParseInt(kv[1].Trim(), out int count);
                if (count > 0)
                    result.Add(new ItemStartEntry(itemId, count));
            }

            return result;
        }
    }
}

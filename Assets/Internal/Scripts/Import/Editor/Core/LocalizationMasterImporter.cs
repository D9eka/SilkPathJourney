using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Core
{
    public static class LocalizationMasterImporter
    {
        private static readonly (string table, string[] prefixes, bool usePlain)[] Tables =
        {
            ("Economy", new[]
            {
                "item.", "item_category.", "city.", "city_type.",
                "building.", "modifier.", "city_modifier.", "culture."
            }, true),
            ("UI", new[] { "UI." }, false),
            ("Events", new[] { "event.", "event_type." }, true),
            ("Theme", new[] { "biome." }, true),
            ("Npc", new[] { "npc.name." }, true),
            ("Quests", new[] { "quest.", "quest_branch." }, true),
            ("Caravan", new[]
            {
                "cart_class.", "cart_upgrade.", "extra_cart.",
                "companion.", "companion_type.", "companion_quality.", "companion_bonus.",
                "caravan_upgrade.", "draft_animal."
            }, true),
        };

        [MenuItem("SPJ/Import/Localization")]
        public static void ImportAll()
        {
            if (IsCompiling()) return;

            try
            {
                EnsureAssetFolder(LOCALIZATION_FOLDER);
                EnsureAssetFolder(LOCALIZATION_LOCALES_FOLDER);
                EnsureAssetFolder(LOCALIZATION_TABLES_FOLDER);

                string locCsv = CsvPath("localization.csv");
                int totalEntries = 0;

                foreach (var (table, prefixes, usePlain) in Tables)
                {
                    var entries = new Dictionary<string, LocalizationImporter.LocalizationEntry>();

                    foreach (string prefix in prefixes)
                    {
                        if (usePlain)
                            LocalizationImporter.CollectFromCsvPlainLocales(locCsv, "key", prefix, entries);
                        else
                            LocalizationImporter.CollectFromCsvWithPrefix(locCsv, "key", prefix, entries);
                    }

                    LocalizationImporter.Import(entries, table,
                        LOCALIZATION_TABLES_FOLDER, LOCALIZATION_LOCALES_FOLDER);
                    totalEntries += entries.Count;
                }

                AssetDatabase.SaveAssets();
                Debug.Log($"[SPJ] All localization imported: {totalEntries} entries across {Tables.Length} tables.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}

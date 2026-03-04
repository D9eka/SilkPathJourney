using System;
using System.Collections.Generic;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.UI.Screens.Trader;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Import.Editor.Trader
{
    public static class TraderImporter
    {
        private const string SKILLS_CSV = "skills.csv";
        private const string CATALOG_FOLDER = ImportHelpers.GENERATED_DATA_FOLDER + "/UI";
        private const string LOC_TABLE = "UI";

        [MenuItem("SPJ/Import/Trader/Import Data")]
        public static void Import()
        {
            if (ImportHelpers.IsCompiling())
                return;

            var skills = ReadSkills();
            var profiles = BuildProfileEntries();

            ImportHelpers.EnsureAssetFolder(CATALOG_FOLDER);
            var catalog = ImportHelpers.LoadOrCreateAsset<TraderUICatalog>(CATALOG_FOLDER + "/TraderUICatalog.asset");
            catalog.ApplyImport(skills, profiles);

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SPJ] TraderImporter: {skills.Count} skills, {profiles.Count} profile items imported.");
        }

        private static List<TraderUICatalog.SkillLocEntry> ReadSkills()
        {
            string csvPath = ImportHelpers.CsvPath(SKILLS_CSV);
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            var result = new List<TraderUICatalog.SkillLocEntry>();

            if (rows.Count == 0)
                return result;

            string[] header = rows[0];
            int idIdx = ImportHelpers.FindColumnIndex(header, "skill_id");
            int nameIdx = ImportHelpers.FindColumnIndex(header, "name_key");
            int descIdx = ImportHelpers.FindColumnIndex(header, "description_key");

            if (idIdx < 0 || nameIdx < 0 || descIdx < 0)
            {
                Debug.LogError("[SPJ] TraderImporter: Missing columns in skills.csv");
                return result;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = ImportHelpers.GetField(rows[i], idIdx).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                if (!Enum.TryParse(id, true, out SkillType skillType) || skillType == SkillType.None)
                {
                    Debug.LogWarning($"[SPJ] TraderImporter: Unknown skill '{id}' at row {i + 1}");
                    continue;
                }

                string nameKey = ImportHelpers.GetField(rows[i], nameIdx).Trim();
                string descKey = ImportHelpers.GetField(rows[i], descIdx).Trim();

                result.Add(new TraderUICatalog.SkillLocEntry
                {
                    Type = skillType,
                    Name = ImportHelpers.MakeLocalizedString(nameKey, LOC_TABLE),
                    Description = ImportHelpers.MakeLocalizedString(descKey, LOC_TABLE)
                });
            }

            return result;
        }

        private static List<TraderUICatalog.ProfileLocEntry> BuildProfileEntries()
        {
            return new List<TraderUICatalog.ProfileLocEntry>
            {
                new()
                {
                    Id = "cart",
                    Header = ImportHelpers.MakeLocalizedString("UI.Trader.Profile.Cart", LOC_TABLE)
                },
                new()
                {
                    Id = "backstory",
                    Header = ImportHelpers.MakeLocalizedString("UI.Trader.Profile.Backstory", LOC_TABLE)
                }
            };
        }
    }
}

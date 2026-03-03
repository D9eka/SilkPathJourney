using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.Npc.Names;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Npc
{
    public static class NpcImporter
    {
        private const string NAME_DATABASE_PATH = DATABASES_FOLDER + "/NameDatabase.asset";
        private const string NPC_LOC_TABLE = "Npc";

        [MenuItem("SPJ/Import/Npc/Import Names")]
        public static void ImportNames()
        {
            if (IsCompiling()) return;

            try
            {
                EnsureAssetFolder(DATABASES_FOLDER);
                EnsureAssetFolder(LOCALIZATION_TABLES_FOLDER);
                EnsureAssetFolder(LOCALIZATION_LOCALES_FOLDER);

                Dictionary<string, CultureId> cultureMap =
                    BuildEnumMap<CultureId>("cultures.csv", "culture_id", "enum_name");

                List<NameEntry> entries = ReadNames(cultureMap);

                var locEntries = new Dictionary<string, LocalizationImporter.LocalizationEntry>();
                LocalizationImporter.CollectFromCsvPlainLocales(
                    CsvPath("localization.csv"), "key", "npc.name.", locEntries);
                LocalizationImporter.Import(
                    locEntries, NPC_LOC_TABLE, LOCALIZATION_TABLES_FOLDER, LOCALIZATION_LOCALES_FOLDER);

                NameDatabase db = LoadOrCreateAsset<NameDatabase>(NAME_DATABASE_PATH);

                db.ApplyImport(entries);
                EditorUtility.SetDirty(db);
                AssetDatabase.SaveAssets();

                Debug.Log($"[SPJ] Imported {entries.Count} names + localization into NameDatabase.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SPJ] Name import failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static List<NameEntry> ReadNames(Dictionary<string, CultureId> cultureMap)
        {
            string csvPath = CsvPath("names.csv");
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            List<NameEntry> entries = new();

            if (rows.Count == 0)
            {
                Debug.LogWarning("[SPJ] names.csv is empty or not found.");
                return entries;
            }

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "name_id");
            int cultureIndex = FindColumnIndex(header, "culture_id");
            int nameIndex = FindColumnIndex(header, "name");

            if (idIndex < 0 || cultureIndex < 0 || nameIndex < 0)
            {
                Debug.LogError("[SPJ] Missing required columns in names.csv (need: name_id, culture_id, name).");
                return entries;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string[] row = rows[i];
                string id = GetField(row, idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                string cultureKey = GetField(row, cultureIndex).Trim();
                CultureId culture = cultureMap.TryGetValue(cultureKey, out CultureId c)
                    ? c
                    : CultureId.None;

                if (culture == CultureId.None)
                    Debug.LogWarning($"[SPJ] Unknown culture_id '{cultureKey}' for name '{id}' at row {i + 1}.");

                string name = GetField(row, nameIndex).Trim();
                entries.Add(new NameEntry(id, culture, name));
            }

            return entries;
        }
    }
}

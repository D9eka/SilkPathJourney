using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.UI.Theme;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Theme
{
    public static class ThemeImporter
    {
        private const string PALETTES_FOLDER = GENERATED_DATA_FOLDER + "/BiomePalettes";
        private const string LOCALIZATION_TABLE_NAME = "Theme";

        [MenuItem("SPJ/Import/Theme")]
        public static void ImportColors()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("[SPJ] Cannot import while Unity is compiling.");
                return;
            }

            try
            {
                // 1. Generate Biome enum
                BiomeGenerator.Generate();

                // 2. Ensure folders
                EnsureAssetFolder(PALETTES_FOLDER);
                EnsureAssetFolder(DATABASES_FOLDER);

                // 3. Build enum map
                var biomeMap = BuildEnumMap<Biome>("biome_palettes.csv", "biome_id", "enum_name");

                // 4. Import palettes
                var locEntries = new Dictionary<string, LocalizationImporter.LocalizationEntry>();
                var palettes = BiomePalettesTable.Import(biomeMap, locEntries);

                // 5. Create/update BiomePaletteMap
                UpdateBiomePaletteMap(palettes);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[SPJ] Theme colors imported: {palettes.Count} palettes.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [MenuItem("SPJ/Import/Localization")]
        public static void ImportLocalization()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("[SPJ] Cannot import while Unity is compiling.");
                return;
            }

            try
            {
                EnsureAssetFolder(LOCALIZATION_FOLDER);
                EnsureAssetFolder(LOCALIZATION_LOCALES_FOLDER);
                EnsureAssetFolder(LOCALIZATION_TABLES_FOLDER);

                var locEntries = new Dictionary<string, LocalizationImporter.LocalizationEntry>();
                LocalizationImporter.CollectFromCsv(
                    CsvPath("biome_palettes.csv"), "name_key", locEntries);

                LocalizationImporter.Import(locEntries, LOCALIZATION_TABLE_NAME,
                    LOCALIZATION_TABLES_FOLDER, LOCALIZATION_LOCALES_FOLDER);

                AssetDatabase.SaveAssets();
                Debug.Log($"[SPJ] Theme localization imported: {locEntries.Count} entries.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void UpdateBiomePaletteMap(List<(Biome biome, UiColorPalette palette)> palettes)
        {
            string assetPath = $"{DATABASES_FOLDER}/BiomePaletteMap.asset";
            BiomePaletteMap map = AssetDatabase.LoadAssetAtPath<BiomePaletteMap>(assetPath);
            if (map == null)
            {
                map = ScriptableObject.CreateInstance<BiomePaletteMap>();
                AssetDatabase.CreateAsset(map, assetPath);
            }

            var entries = new BiomePaletteMap.Entry[palettes.Count];
            UiColorPalette fallback = null;

            for (int i = 0; i < palettes.Count; i++)
            {
                entries[i] = new BiomePaletteMap.Entry
                {
                    Biome = palettes[i].biome,
                    Palette = palettes[i].palette,
                };

                if (palettes[i].biome == Biome.Plains)
                    fallback = palettes[i].palette;
            }

            if (fallback == null && palettes.Count > 0)
                fallback = palettes[0].palette;

            map.ApplyImport(entries, fallback);
            EditorUtility.SetDirty(map);
        }
    }
}

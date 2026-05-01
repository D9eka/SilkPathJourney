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

        [MenuItem("SPJ/Import/Theme/Generate")]
        public static void Generate()
        {
            BiomeGenerator.Generate();
        }

        [MenuItem("SPJ/Import/Theme/Import")]
        public static void Import()
        {
            if (IsCompiling()) return;

            try
            {
                // 2. Ensure folders
                EnsureAssetFolder(PALETTES_FOLDER);
                EnsureAssetFolder(DATABASES_FOLDER);

                // 3. Build enum map
                var biomeMap = BuildEnumMap<Biome>("biome_palettes.csv", "biome_id", "enum_name");

                // 4. Import palettes
                var palettes = BiomePalettesTable.Import(biomeMap);

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

        private static void UpdateBiomePaletteMap(List<(Biome biome, UiColorPalette palette)> palettes)
        {
            string assetPath = $"{DATABASES_FOLDER}/BiomePaletteMap.asset";
            BiomePaletteMap map = LoadOrCreateAsset<BiomePaletteMap>(assetPath);

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

using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.UI.Theme;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Theme
{
    public static class BiomePalettesTable
    {
        private const string OUTPUT_FOLDER = GENERATED_DATA_FOLDER + "/BiomePalettes";

        private static readonly string[] ColorColumns =
        {
            "WindowBackground", "WindowContainer", "WindowElement",
            "WindowText", "WindowElementText", "WindowContainerText",
            "HudElementBackground", "HudElementText",
            "OverlayColor", "IconPrimary", "IconSecondary", "TimeButtonBackground",
            "SliderBackground", "SliderFillPositive", "SliderFillNegative",
            "ButtonSelectedBorder",
            "RoadHighlight", "ValuePositive", "ValueNegative", "AccentHighlight",
            "CityMarker",
        };

        public static List<(Biome biome, UiColorPalette palette)> Import(
            Dictionary<string, Biome> biomeMap)
        {
            EnsureAssetFolder(OUTPUT_FOLDER);

            string csvPath = CsvPath("biome_palettes.csv");
            List<string[]> rows = CsvReader.ReadFile(csvPath);
            var result = new List<(Biome, UiColorPalette)>();

            if (rows.Count == 0)
            {
                Debug.LogError("[SPJ] biome_palettes.csv is empty.");
                return result;
            }

            string[] header = rows[0];
            int idIndex = FindColumnIndex(header, "biome_id");

            if (idIndex < 0)
            {
                Debug.LogError("[SPJ] Missing 'biome_id' column in biome_palettes.csv");
                return result;
            }

            int[] colorIndices = new int[ColorColumns.Length];
            for (int c = 0; c < ColorColumns.Length; c++)
            {
                colorIndices[c] = FindColumnIndex(header, ColorColumns[c]);
                if (colorIndices[c] < 0)
                    Debug.LogWarning($"[SPJ] Missing color column '{ColorColumns[c]}' in biome_palettes.csv");
            }

            for (int i = 1; i < rows.Count; i++)
            {
                string id = GetField(rows[i], idIndex).Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                if (!biomeMap.TryGetValue(id, out Biome biome))
                {
                    Debug.LogWarning($"[SPJ] Unknown biome_id '{id}' at row {i + 1}");
                    continue;
                }

                Color[] colors = new Color[ColorColumns.Length];
                for (int c = 0; c < ColorColumns.Length; c++)
                {
                    string hex = GetField(rows[i], colorIndices[c]).Trim();
                    if (!string.IsNullOrEmpty(hex) && ColorUtility.TryParseHtmlString(hex, out Color parsed))
                        colors[c] = parsed;
                    else
                        colors[c] = Color.magenta;
                }

                UiColorPalette palette = LoadOrCreateAsset<UiColorPalette>(OUTPUT_FOLDER, id);
                palette.ApplyImport(
                    colors[0], colors[1], colors[2], colors[3],
                    colors[4], colors[5], colors[6], colors[7],
                    colors[8], colors[9], colors[10], colors[11],
                    colors[12], colors[13], colors[14], colors[15],
                    colors[16], colors[17], colors[18], colors[19],
                    colors[20]);

                EditorUtility.SetDirty(palette);
                result.Add((biome, palette));
            }

            return result;
        }
    }
}

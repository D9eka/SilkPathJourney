using System.IO;
using Internal.Scripts.Import.Editor.Core;

namespace Internal.Scripts.Import.Editor.Theme
{
    public static class BiomeGenerator
    {
        public static bool Generate()
        {
            var spec = new EnumGenerator.EnumSpec(
                "Biome",
                ImportHelpers.CsvPath("biome_palettes.csv"),
                "biome_id",
                "enum_name",
                Path.Combine(Directory.GetCurrentDirectory(),
                    "Assets/Internal/Scripts/Economy/Generated/Biome.gen.cs"),
                "Unknown",
                "Internal.Scripts.Economy.Generated"
            );

            return EnumGenerator.GenerateFromSpec(spec);
        }
    }
}

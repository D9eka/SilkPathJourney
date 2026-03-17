using System.IO;
using Internal.Scripts.Import.Editor.Core;

namespace Internal.Scripts.Import.Editor.Caravan.Generators
{
    public static class CompanionQualityGenerator
    {
        public static bool Generate()
        {
            var spec = new EnumGenerator.EnumSpec(
                "CompanionQuality",
                ImportHelpers.CsvPath("companion_quality_levels.csv"),
                "quality_id",
                "enum_name",
                Path.Combine(Directory.GetCurrentDirectory(),
                    "Assets/Internal/Scripts/Caravan/Generated/CompanionQuality.gen.cs"),
                "Unknown",
                "Internal.Scripts.Caravan.Generated"
            );

            return EnumGenerator.GenerateFromSpec(spec);
        }
    }
}

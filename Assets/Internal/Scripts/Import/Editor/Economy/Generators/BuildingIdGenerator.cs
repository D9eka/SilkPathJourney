using System.IO;
using Internal.Scripts.Import.Editor.Core;

namespace Internal.Scripts.Import.Editor.Economy.Generators
{
    public static class BuildingIdGenerator
    {
        public static bool Generate()
        {
            var spec = new EnumGenerator.EnumSpec(
                "BuildingId",
                ImportHelpers.CsvPath("buildings.csv"),
                "building_id",
                "enum_name",
                Path.Combine(Directory.GetCurrentDirectory(),
                    "Assets/Internal/Scripts/Economy/Generated/BuildingId.gen.cs"),
                "Unknown",
                "Internal.Scripts.Economy.Generated"
            );

            return EnumGenerator.GenerateFromSpec(spec);
        }
    }
}
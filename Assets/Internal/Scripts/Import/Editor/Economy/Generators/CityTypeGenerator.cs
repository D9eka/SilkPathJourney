using System.IO;
using Internal.Scripts.Import.Editor.Core;

namespace Internal.Scripts.Import.Editor.Economy.Generators
{
    public static class CityTypeGenerator
    {
        public static bool Generate()
        {
            var spec = new EnumGenerator.EnumSpec(
                "CityType",
                ImportHelpers.CsvPath("city_types.csv"),
                "city_type_id",
                "enum_name",
                Path.Combine(Directory.GetCurrentDirectory(),
                    "Assets/Internal/Scripts/Economy/Generated/CityType.gen.cs"),
                "Unknown",
                "Internal.Scripts.Economy.Generated"
            );

            return EnumGenerator.GenerateFromSpec(spec);
        }
    }
}

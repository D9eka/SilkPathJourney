using System.IO;
using Internal.Scripts.Import.Editor.Core;

namespace Internal.Scripts.Import.Editor.Economy.Generators
{
    public static class CultureIdGenerator
    {
        public static bool Generate()
        {
            var spec = new EnumGenerator.EnumSpec(
                "CultureId",
                ImportHelpers.CsvPath("cultures.csv"),
                "culture_id",
                "enum_name",
                Path.Combine(Directory.GetCurrentDirectory(),
                    "Assets/Internal/Scripts/Economy/Generated/CultureId.gen.cs"),
                "None",
                "Internal.Scripts.Economy.Generated"
            );

            return EnumGenerator.GenerateFromSpec(spec);
        }
    }
}

using System.IO;
using Internal.Scripts.Import.Editor.Core;

namespace Internal.Scripts.Import.Editor.Caravan.Generators
{
    public static class DraftAnimalTypeGenerator
    {
        public static bool Generate()
        {
            var spec = new EnumGenerator.EnumSpec(
                "DraftAnimalType",
                ImportHelpers.CsvPath("draft_animals.csv"),
                "animal_id",
                "enum_name",
                Path.Combine(Directory.GetCurrentDirectory(),
                    "Assets/Internal/Scripts/Caravan/Generated/DraftAnimalType.gen.cs"),
                "Unknown",
                "Internal.Scripts.Caravan.Generated"
            );

            return EnumGenerator.GenerateFromSpec(spec);
        }
    }
}

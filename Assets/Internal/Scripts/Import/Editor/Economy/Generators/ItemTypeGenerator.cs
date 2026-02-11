using System.IO;
using Internal.Scripts.Import.Editor.Core;

namespace Internal.Scripts.Import.Editor.Economy.Generators
{
    public static class ItemTypeGenerator
    {
        public static bool Generate()
        {
            var spec = new EnumGenerator.EnumSpec(
                "ItemType",
                ImportHelpers.CsvPath("item_categories.csv"),
                "category_id",
                "enum_name",
                Path.Combine(Directory.GetCurrentDirectory(),
                    "Assets/Internal/Scripts/Economy/Generated/ItemType.gen.cs"),
                "Unknown",
                "Internal.Scripts.Economy.Generated"
            );

            return EnumGenerator.GenerateFromSpec(spec);
        }
    }
}

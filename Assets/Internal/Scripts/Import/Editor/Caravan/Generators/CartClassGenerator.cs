using System.IO;
using Internal.Scripts.Import.Editor.Core;

namespace Internal.Scripts.Import.Editor.Caravan.Generators
{
    public static class CartClassGenerator
    {
        public static bool Generate()
        {
            var spec = new EnumGenerator.EnumSpec(
                "CartClass",
                ImportHelpers.CsvPath("cart_classes.csv"),
                "class_id",
                "enum_name",
                Path.Combine(Directory.GetCurrentDirectory(),
                    "Assets/Internal/Scripts/Caravan/Generated/CartClass.gen.cs"),
                "Unknown",
                "Internal.Scripts.Caravan.Generated"
            );

            return EnumGenerator.GenerateFromSpec(spec);
        }
    }
}

using System.IO;
using Internal.Scripts.Import.Editor.Core;

namespace Internal.Scripts.Import.Editor.Caravan.Generators
{
    public static class CartUpgradeLevelGenerator
    {
        public static bool Generate()
        {
            var spec = new EnumGenerator.EnumSpec(
                "CartUpgradeLevel",
                ImportHelpers.CsvPath("cart_upgrade_levels.csv"),
                "level_id",
                "enum_name",
                Path.Combine(Directory.GetCurrentDirectory(),
                    "Assets/Internal/Scripts/Caravan/Generated/CartUpgradeLevel.gen.cs"),
                "Unknown",
                "Internal.Scripts.Caravan.Generated"
            );

            return EnumGenerator.GenerateFromSpec(spec);
        }
    }
}

using System.IO;
using Internal.Scripts.Import.Editor.Core;

namespace Internal.Scripts.Import.Editor.Caravan.Generators
{
    public static class CaravanUpgradeTypeGenerator
    {
        public static bool Generate()
        {
            var spec = new EnumGenerator.EnumSpec(
                "CaravanUpgradeType",
                ImportHelpers.CsvPath("caravan_upgrades.csv"),
                "upgrade_id",
                "enum_name",
                Path.Combine(Directory.GetCurrentDirectory(),
                    "Assets/Internal/Scripts/Caravan/Generated/CaravanUpgradeType.gen.cs"),
                "Unknown",
                "Internal.Scripts.Caravan.Generated"
            );

            return EnumGenerator.GenerateFromSpec(spec);
        }
    }
}

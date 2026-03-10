using System.IO;
using Internal.Scripts.Import.Editor.Core;
using UnityEditor;

namespace Internal.Scripts.Import.Editor.Languages
{
    public static class LanguageTypeGenerator
    {
        [MenuItem("SPJ/Import/Languages")]
        public static bool Generate()
        {
            var spec = new EnumGenerator.EnumSpec(
                "LanguageType",
                ImportHelpers.CsvPath("languages.csv"),
                "language_id",
                "enum_name",
                Path.Combine(Directory.GetCurrentDirectory(),
                    "Assets/Internal/Scripts/Player/Languages/Generated/LanguageType.gen.cs"),
                "None",
                "Internal.Scripts.Player.Languages.Generated"
            );

            return EnumGenerator.GenerateFromSpec(spec);
        }
    }
}

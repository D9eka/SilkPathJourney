using System.IO;
using Internal.Scripts.Import.Editor.Core;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Events.Generators
{
    public static class EventConditionTypeGenerator
    {
        private const string ENUM_NAME = "EventConditionType";
        private const string CSV_FILE = "event_condition_types.csv";
        private const string ID_COLUMN = "type_id";
        private const string ENUM_COLUMN = "enum_name";
        private const string OUTPUT_PATH = "Assets/Internal/Scripts/Events/Generated/EventConditionType.gen.cs";
        private const string NAMESPACE = "Internal.Scripts.Events.Data";
        private const string ZERO_VALUE = "None";

        public static bool Generate()
        {
            string projectRoot = Directory.GetCurrentDirectory();

            EnumGenerator.EnumSpec spec = new(
                ENUM_NAME,
                Path.Combine(projectRoot, CSV_FOLDER, CSV_FILE),
                ID_COLUMN,
                ENUM_COLUMN,
                Path.Combine(projectRoot, OUTPUT_PATH),
                ZERO_VALUE,
                NAMESPACE
            );

            return EnumGenerator.GenerateFromSpec(spec);
        }
    }
}

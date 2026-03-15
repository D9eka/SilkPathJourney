using System.IO;
using Internal.Scripts.Import.Editor.Core;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Quests.Generators
{
    public static class QuestBranchGenerator
    {
        private const string ENUM_NAME = "QuestBranch";
        private const string CSV_FILE = "quest_branches.csv";
        private const string ID_COLUMN = "branch_id";
        private const string ENUM_COLUMN = "enum_name";
        private const string OUTPUT_PATH = "Assets/Internal/Scripts/Quests/Generated/QuestBranch.gen.cs";
        private const string NAMESPACE = "Internal.Scripts.Quests.Generated";
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

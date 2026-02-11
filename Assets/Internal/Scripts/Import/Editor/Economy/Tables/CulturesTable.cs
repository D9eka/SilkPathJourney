using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy.Tables
{
    public static class CulturesTable
    {
        public static Dictionary<string, CultureId> Read(
            Dictionary<string, LocalizationImporter.LocalizationEntry> locEntries)
        {
            var map = BuildEnumMap<CultureId>("cultures.csv", "culture_id", "enum_name");
            LocalizationImporter.CollectFromCsv(CsvPath("cultures.csv"), "name_key", locEntries);
            return map;
        }
    }
}

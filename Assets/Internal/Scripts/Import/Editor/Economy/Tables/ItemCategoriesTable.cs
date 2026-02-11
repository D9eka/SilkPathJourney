using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Import.Editor.Core;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy.Tables
{
    public static class ItemCategoriesTable
    {
        public static Dictionary<string, ItemType> Read(
            Dictionary<string, LocalizationImporter.LocalizationEntry> locEntries)
        {
            var map = BuildEnumMap<ItemType>("item_categories.csv", "category_id", "enum_name");
            LocalizationImporter.CollectFromCsv(CsvPath("item_categories.csv"), "name_key", locEntries);
            return map;
        }
    }
}

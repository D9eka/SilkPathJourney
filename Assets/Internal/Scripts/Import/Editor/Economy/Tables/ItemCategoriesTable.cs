using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Economy.Tables
{
    public static class ItemCategoriesTable
    {
        public static Dictionary<string, ItemType> Read()
        {
            return BuildEnumMap<ItemType>("item_categories.csv", "category_id", "enum_name");
        }
    }
}

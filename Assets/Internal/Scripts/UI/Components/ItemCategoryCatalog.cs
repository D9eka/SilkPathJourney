using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    [CreateAssetMenu(menuName = "SPJ/UI/Item Category Catalog")]
    public class ItemCategoryCatalog : ScriptableObject
    {
        [SerializeField] private ItemCategoryEntry[] _entries;

        private Dictionary<ItemType, ItemCategoryEntry> _lookup;

        public ItemCategoryEntry Get(ItemType category)
        {
            if (_lookup == null)
                _lookup = BuildLookup();

            _lookup.TryGetValue(category, out ItemCategoryEntry result);
            return result;
        }

        private Dictionary<ItemType, ItemCategoryEntry> BuildLookup()
        {
            Dictionary<ItemType, ItemCategoryEntry> lookup = new();
            if (_entries != null)
            {
                foreach (ItemCategoryEntry entry in _entries)
                    lookup[entry.Category] = entry;
            }
            return lookup;
        }
    }
}

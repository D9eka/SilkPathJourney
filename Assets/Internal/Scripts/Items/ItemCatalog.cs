using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Localization;

namespace Internal.Scripts.Items
{
    public sealed class ItemCatalog
    {
        private readonly Dictionary<string, ItemData> _itemsById = new();

        public ItemCatalog(EconomyDatabase economyDatabase)
        {
            if (economyDatabase == null || economyDatabase.Items == null)
                return;

            foreach (ItemData item in economyDatabase.Items)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id))
                    continue;

                _itemsById.TryAdd(item.Id, item);
            }
        }

        public ItemData GetItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return null;

            _itemsById.TryGetValue(itemId, out ItemData item);
            return item;
        }

        public string ResolveItemName(string itemId)
        {
            ItemData item = GetItem(itemId);
            if (item == null)
                return itemId ?? string.Empty;

            string fallback = string.IsNullOrWhiteSpace(item.Id) ? string.Empty : item.Id;
            return LocalizedStringResolver.Resolve(item.Name, fallback, $"Item.{item.Id}.Name");
        }

        public float GetItemWeight(string itemId)
        {
            ItemData item = GetItem(itemId);
            return item != null ? item.WeightKg : 0f;
        }

        public int GetItemPrice(string itemId)
        {
            ItemData item = GetItem(itemId);
            return item != null ? item.BasePrice : 0;
        }
    }
}

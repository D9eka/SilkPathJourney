using System.Collections.Generic;
using Internal.Scripts.Economy.Save.Models;

namespace Internal.Scripts.Items
{
    public sealed class ItemRowsBuilder
    {
        private readonly ItemCatalog _itemCatalog;

        public ItemRowsBuilder(ItemCatalog itemCatalog)
        {
            _itemCatalog = itemCatalog;
        }

        public IReadOnlyList<ItemRowData> BuildRows(InventoryState inventory, bool includePrice)
        {
            List<ItemRowData> rows = new();
            if (inventory == null || inventory.Items == null)
                return rows;

            foreach (ItemStackState stack in inventory.Items)
            {
                if (stack == null || stack.Count <= 0)
                    continue;

                AddRow(rows, stack.ItemId, stack.Count, includePrice);
            }

            return rows;
        }

        public IReadOnlyList<ItemRowData> BuildRows(IReadOnlyDictionary<string, int> counts, bool includePrice)
        {
            List<ItemRowData> rows = new();
            if (counts == null)
                return rows;

            foreach (KeyValuePair<string, int> kvp in counts)
            {
                if (kvp.Value <= 0)
                    continue;

                AddRow(rows, kvp.Key, kvp.Value, includePrice);
            }

            return rows;
        }

        public IReadOnlyList<ItemRowData> BuildRemainingRows(IReadOnlyDictionary<string, int> baseCounts,
            IReadOnlyDictionary<string, int> reserved, bool includePrice)
        {
            List<ItemRowData> rows = new();
            if (baseCounts == null)
                return rows;

            foreach (KeyValuePair<string, int> kvp in baseCounts)
            {
                int used = 0;
                reserved?.TryGetValue(kvp.Key, out used);
                int remaining = kvp.Value - used;
                if (remaining <= 0)
                    continue;

                AddRow(rows, kvp.Key, remaining, includePrice);
            }

            return rows;
        }

        public int ComputeItemsHash(InventoryState inventory)
        {
            if (inventory == null || inventory.Items == null)
                return 0;

            int hash = 17;
            foreach (ItemStackState stack in inventory.Items)
            {
                if (stack == null || stack.Count <= 0)
                    continue;

                hash = System.HashCode.Combine(hash, stack.ItemId, stack.Count);
            }

            return hash;
        }

        public int ComputeCountsHash(IReadOnlyDictionary<string, int> counts)
        {
            if (counts == null)
                return 0;

            int hash = 17;
            foreach (KeyValuePair<string, int> kvp in counts)
            {
                if (kvp.Value <= 0)
                    continue;

                hash = System.HashCode.Combine(hash, kvp.Key, kvp.Value);
            }

            return hash;
        }

        public int ComputeRemainingHash(IReadOnlyDictionary<string, int> baseCounts, IReadOnlyDictionary<string, int> reserved)
        {
            if (baseCounts == null)
                return 0;

            int hash = 17;
            foreach (KeyValuePair<string, int> kvp in baseCounts)
            {
                int used = 0;
                reserved?.TryGetValue(kvp.Key, out used);
                int remaining = kvp.Value - used;
                if (remaining <= 0)
                    continue;

                hash = System.HashCode.Combine(hash, kvp.Key, remaining);
            }

            return hash;
        }

        private void AddRow(List<ItemRowData> rows, string itemId, int count, bool includePrice)
        {
            string name = _itemCatalog.ResolveItemName(itemId);
            if (count > 1)
                name = $"{name} x{count}";

            string weightText = string.Empty;
            string priceText = string.Empty;

            float weight = _itemCatalog.GetItemWeight(itemId);
            if (weight > 0f)
            {
                float totalWeight = weight * count;
                weightText = totalWeight.ToString("0.##");
            }

            if (includePrice)
                priceText = _itemCatalog.GetItemPrice(itemId).ToString();

            rows.Add(new ItemRowData(itemId, count, name, weightText, priceText));
        }
    }
}

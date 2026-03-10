using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Trading;

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
            return BuildRows(inventory, includePrice ? new Func<string, int>(_itemCatalog.GetItemPrice) : null);
        }

        public IReadOnlyList<ItemRowData> BuildRows(InventoryState inventory, Func<string, int> priceResolver)
        {
            List<ItemRowData> rows = new();
            if (inventory == null || inventory.Items == null)
                return rows;

            foreach (ItemStackState stack in inventory.Items)
            {
                if (stack == null || stack.Count <= 0)
                    continue;

                AddRow(rows, stack.ItemId, stack.Count, priceResolver);
            }

            SortRows(rows);
            return rows;
        }

        public IReadOnlyList<ItemRowData> BuildRows(IReadOnlyDictionary<string, int> counts, bool includePrice)
        {
            return BuildRows(counts, includePrice ? new Func<string, int>(_itemCatalog.GetItemPrice) : null);
        }

        public IReadOnlyList<ItemRowData> BuildRows(IReadOnlyDictionary<string, int> counts, Func<string, int> priceResolver)
        {
            return BuildRows(counts, priceResolver, null);
        }

        public IReadOnlyList<ItemRowData> BuildRows(IReadOnlyDictionary<string, int> counts,
            Func<string, int> priceResolver, Func<string, PriceBreakdown> breakdownResolver)
        {
            List<ItemRowData> rows = new();
            if (counts == null)
                return rows;

            foreach (KeyValuePair<string, int> kvp in counts)
            {
                if (kvp.Value <= 0)
                    continue;

                AddRow(rows, kvp.Key, kvp.Value, priceResolver, breakdownResolver);
            }

            SortRows(rows);
            return rows;
        }

        public IReadOnlyList<ItemRowData> BuildRemainingRows(IReadOnlyDictionary<string, int> baseCounts,
            IReadOnlyDictionary<string, int> reserved, bool includePrice)
        {
            return BuildRemainingRows(baseCounts, reserved, includePrice ? new Func<string, int>(_itemCatalog.GetItemPrice) : null);
        }

        public IReadOnlyList<ItemRowData> BuildRemainingRows(IReadOnlyDictionary<string, int> baseCounts,
            IReadOnlyDictionary<string, int> reserved, Func<string, int> priceResolver)
        {
            return BuildRemainingRows(baseCounts, reserved, priceResolver, null);
        }

        public IReadOnlyList<ItemRowData> BuildRemainingRows(IReadOnlyDictionary<string, int> baseCounts,
            IReadOnlyDictionary<string, int> reserved, Func<string, int> priceResolver,
            Func<string, PriceBreakdown> breakdownResolver)
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

                AddRow(rows, kvp.Key, remaining, priceResolver, breakdownResolver);
            }

            SortRows(rows);
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

        private static void SortRows(List<ItemRowData> rows)
        {
            rows.Sort((a, b) =>
            {
                bool aSupplies = a.ItemId == SuppliesItemId.Value;
                bool bSupplies = b.ItemId == SuppliesItemId.Value;
                if (aSupplies != bSupplies)
                    return aSupplies ? -1 : 1;
                return string.Compare(a.ItemId, b.ItemId, StringComparison.Ordinal);
            });
        }

        private void AddRow(List<ItemRowData> rows, string itemId, int count,
            Func<string, int> priceResolver, Func<string, PriceBreakdown> breakdownResolver = null)
        {
            string name = _itemCatalog.ResolveItemName(itemId);
            if (count > 1)
                name = $"{name} x{count}";

            string weightText = string.Empty;
            string priceText = string.Empty;
            string tooltipTitle = null;
            string tooltipText = null;

            float weight = _itemCatalog.GetItemWeight(itemId);
            if (weight > 0f)
            {
                float totalWeight = weight * count;
                weightText = totalWeight.ToString("0.##");
            }

            if (priceResolver != null)
                priceText = priceResolver(itemId).ToString();

            if (breakdownResolver != null)
            {
                PriceBreakdown breakdown = breakdownResolver(itemId);
                (tooltipTitle, tooltipText) = PriceTooltipFormatter.Format(breakdown);
            }

            rows.Add(new ItemRowData(itemId, count, name, weightText, priceText, tooltipTitle, tooltipText));
        }
    }
}

using System.Collections.Generic;
using Internal.Scripts.Economy.Save.Models;

namespace Internal.Scripts.Items
{
    public sealed class ItemWeightCalculator
    {
        private readonly ItemCatalog _itemCatalog;

        public ItemWeightCalculator(ItemCatalog itemCatalog)
        {
            _itemCatalog = itemCatalog;
        }

        public float CalculateInventoryWeight(InventoryState inventory)
        {
            if (inventory == null || inventory.Items == null)
                return 0f;

            float total = 0f;
            foreach (ItemStackState stack in inventory.Items)
            {
                if (stack == null || stack.Count <= 0)
                    continue;

                float weight = _itemCatalog.GetItemWeight(stack.ItemId);
                if (weight <= 0f)
                    continue;

                total += weight * stack.Count;
            }

            return total;
        }

        public float CalculateWeightDelta(IReadOnlyDictionary<string, int> counts)
        {
            float total = 0f;
            if (counts == null)
                return total;

            foreach (KeyValuePair<string, int> kvp in counts)
            {
                if (kvp.Value <= 0)
                    continue;

                float weight = _itemCatalog.GetItemWeight(kvp.Key);
                if (weight <= 0f)
                    continue;

                total += weight * kvp.Value;
            }

            return total;
        }
    }
}

using System.Collections.Generic;
using Internal.Scripts.Economy.Save.Models;

namespace Internal.Scripts.Inventory
{
    public static class InventoryStateMutator
    {
        public static void ApplyTrade(InventoryState state,
            Dictionary<string, int> toAdd, Dictionary<string, int> toRemove)
        {
            if (state == null)
                return;

            foreach (KeyValuePair<string, int> kvp in toAdd)
                AddItems(state, kvp.Key, kvp.Value);

            foreach (KeyValuePair<string, int> kvp in toRemove)
                RemoveItems(state, kvp.Key, kvp.Value);
        }

        public static void AddItems(InventoryState inventory, string itemId, int count)
        {
            if (inventory == null || string.IsNullOrWhiteSpace(itemId) || count <= 0)
                return;

            ItemStackState stack = inventory.Items.Find(s => s.ItemId == itemId);
            if (stack != null)
            {
                stack.Count += count;
            }
            else
            {
                inventory.Items.Add(new ItemStackState
                {
                    ItemId = itemId,
                    Count = count
                });
            }
        }

        public static int GetItemCount(InventoryState inventory, string itemId)
        {
            if (inventory?.Items == null)
                return 0;
            ItemStackState stack = inventory.Items.Find(s => s.ItemId == itemId);
            return stack?.Count ?? 0;
        }

        public static void RemoveItems(InventoryState inventory, string itemId, int count)
        {
            if (inventory == null || string.IsNullOrWhiteSpace(itemId) || count <= 0)
                return;

            ItemStackState stack = inventory.Items.Find(s => s.ItemId == itemId);
            if (stack == null)
                return;

            stack.Count -= count;
            if (stack.Count <= 0)
                inventory.Items.Remove(stack);
        }
    }
}

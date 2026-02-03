using Internal.Scripts.Economy.Save.Models;

namespace Internal.Scripts.Inventory
{
    public static class InventoryStateMutator
    {
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

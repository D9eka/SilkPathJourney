using System.Collections.Generic;

using Internal.Scripts.Items;

namespace Internal.Scripts.Inventory
{
    public readonly struct InventoryViewState
    {
        public readonly IReadOnlyList<ItemRowData> Items;
        public readonly int ItemsHash;
        public readonly int Money;
        public readonly float CurrentWeight;
        public readonly float MaxWeight;

        public InventoryViewState(IReadOnlyList<ItemRowData> items, int itemsHash, int money, float currentWeight, float maxWeight)
        {
            Items = items;
            ItemsHash = itemsHash;
            Money = money;
            CurrentWeight = currentWeight;
            MaxWeight = maxWeight;
        }
    }
}

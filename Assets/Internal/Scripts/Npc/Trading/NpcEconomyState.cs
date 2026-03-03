using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Save.Models;

namespace Internal.Scripts.Npc.Trading
{
    [Serializable]
    public sealed class NpcEconomyState
    {
        public string Name;
        public string NameId;
        public int Money;
        public InventoryState Inventory;
        public float CapacityKg;

        public NpcEconomyState(string name, int money, float capacityKg)
        {
            Name = name;
            Money = money;
            CapacityKg = capacityKg;
            Inventory = new InventoryState { Items = new List<ItemStackState>() };
        }
    }
}

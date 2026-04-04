using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Save.Models;

namespace Internal.Scripts.Npc.Data
{
    [Serializable]
    public sealed class NpcEconomyState
    {
        public string Name;
        public string NameId;
        public int Money;
        public InventoryState Inventory;
        public float CapacityKg;
        public NpcArchetype Archetype;
        public NpcExperienceLevel Experience;
        public float Debt;
        public bool InDebt;
        public List<PurchaseRecord> Purchases = new();
        public NpcKnowledgeState Knowledge = new();
        public int LastForageDay;
        public GuildContract? ActiveContract;

        public NpcEconomyState(string name, int money, float capacityKg)
        {
            Name = name;
            Money = money;
            CapacityKg = capacityKg;
            Inventory = new InventoryState { Items = new List<ItemStackState>() };
        }
    }
}

using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Npc.Data;

namespace Internal.Scripts.Npc.Save
{
    [Serializable]
    public class NpcAgentSaveState
    {
        public string Name;
        public string NameId;
        public int Money;
        public InventoryState Inventory = new();
        public float CapacityKg;
        public string CurrentNodeId;
        public string DestinationNodeId;
        public float SpeedMetersPerDay;
        public int ColorIndex;
        public int PrefabIndex;
        public NpcArchetype Archetype;
        public NpcExperienceLevel Experience;
        public float Debt;
        public bool InDebt;
        public List<PurchaseRecord> Purchases = new();
        public NpcKnowledgeState Knowledge = new();
        public int LastForageDay;
        public GuildContract? ActiveContract;
    }
}

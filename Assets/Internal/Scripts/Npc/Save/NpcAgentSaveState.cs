using System;
using Internal.Scripts.Economy.Save.Models;

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
    }
}

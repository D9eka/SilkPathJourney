using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Save.Models;

namespace Internal.Scripts.Economy.Save
{
    [Serializable]
    public class EconomySaveData
    {
        public bool IsInitialized;
        public InventoryState PlayerInventory = new();
        public InventoryState HiddenCompartment = new();
        public PlayerResourceState PlayerResources = new();
        public List<CityInventoryState> CityInventories = new();
        public GuildSaveState Guild = new();
    }
}

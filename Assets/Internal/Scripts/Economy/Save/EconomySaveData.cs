using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Save.Models;

namespace Internal.Scripts.Economy.Save
{
    [Serializable]
    public class EconomySaveData
    {
        public bool IsInitialized;
        public InventoryState PlayerInventory = new();
        public List<CityInventoryState> CityInventories = new();
    }
}

using System;

namespace Internal.Scripts.Economy.Save.Models
{
    [Serializable]
    public class CityInventoryState
    {
        public string CityId;
        public InventoryState Inventory = new();
    }
}

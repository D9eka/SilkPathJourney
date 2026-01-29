using System;

namespace Internal.Scripts.Economy.Save
{
    [Serializable]
    public class CityInventoryState
    {
        public string CityId;
        public InventoryState Inventory = new();
    }
}

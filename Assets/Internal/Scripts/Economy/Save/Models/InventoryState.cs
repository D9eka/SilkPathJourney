using System;
using System.Collections.Generic;

namespace Internal.Scripts.Economy.Save.Models
{
    [Serializable]
    public class InventoryState
    {
        public int Money;
        public List<ItemStackState> Items = new();
    }
}

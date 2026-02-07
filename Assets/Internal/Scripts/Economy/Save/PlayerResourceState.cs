using System;
using System.Collections.Generic;
using System.Linq;

namespace Internal.Scripts.Economy.Save
{
    [Serializable]
    public class PlayerResourceState
    {
        public int Money;
        public float Food = 50f;
        public float AccumulatedDanger;
        public List<CartState> Carts = new();

        public float TotalCapacity => Carts?.Sum(c => c.Capacity) ?? 0f;
    }
}

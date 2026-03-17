using System;

namespace Internal.Scripts.Economy.Save
{
    [Serializable]
    public class CartState
    {
        public string TypeId;
        public float Durability = 100f;
        public float MaxDurability = 100f;
        public float Capacity = 50f;
        public float Speed = 10f;
        public float FoodConsumptionPerDay = 3f;
    }
}

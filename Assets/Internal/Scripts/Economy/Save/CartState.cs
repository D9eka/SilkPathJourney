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
        public int AnimalCount;

        [Obsolete("Use AnimalCount with FoodConsumptionCalculator instead")]
        public float FoodConsumptionPerDay = 3f;
    }
}

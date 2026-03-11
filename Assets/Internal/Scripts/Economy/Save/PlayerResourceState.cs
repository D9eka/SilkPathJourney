using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.UI.Components;

namespace Internal.Scripts.Economy.Save
{
    [Serializable]
    public class PlayerResourceState
    {
        public int Money;
        public float Food = 50f;
        public float AccumulatedDanger;
        public CartState PlayerCart = new();
        public List<CartState> Carts = new();

        public float TotalCapacity => PlayerCart.Capacity + (Carts?.Sum(c => c.Capacity) ?? 0f);
        public float TotalFoodPerDay => PlayerCart.FoodConsumptionPerDay + (Carts?.Sum(c => c.FoodConsumptionPerDay) ?? 0f);

        public float GetValue(ResourceType type) => type switch
        {
            ResourceType.Money => Money,
            ResourceType.Food => Food,
            ResourceType.Danger => AccumulatedDanger,
            ResourceType.PlayerCartDurability => PlayerCart.Durability,
            ResourceType.OtherCartsDurability => Carts.Count > 0
                ? Carts.Sum(c => c.Durability) / Carts.Count : 0f,
            ResourceType.Weight => TotalCapacity,
            _ => 0f
        };
    }
}

using Internal.Scripts.Caravan.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Caravan
{
    [CreateAssetMenu(menuName = "SPJ/Caravan/Extra Cart", fileName = "ExtraCart")]
    public class ExtraCartData : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public ExtraCartType Type { get; private set; }
        [field: SerializeField] public float Capacity { get; private set; }
        [field: SerializeField] public float Durability { get; private set; }
        [field: SerializeField] public float SpeedPenaltyPct { get; private set; }
        [field: SerializeField] public int SuppliesPerDay { get; private set; }
        [field: SerializeField] public int Price { get; private set; }
        [field: SerializeField] public int SellPrice { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; } = new();

#if UNITY_EDITOR
        public void ApplyImport(string id, ExtraCartType type, float capacity, float durability, float speedPenaltyPct, int suppliesPerDay, int price, int sellPrice, LocalizedString name)
        {
            Id = id;
            Type = type;
            Capacity = capacity;
            Durability = durability;
            SpeedPenaltyPct = speedPenaltyPct;
            SuppliesPerDay = suppliesPerDay;
            Price = price;
            SellPrice = sellPrice;
            Name = name;
        }
#endif
    }
}

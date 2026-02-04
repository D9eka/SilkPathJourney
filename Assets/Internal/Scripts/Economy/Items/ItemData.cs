using Internal.Scripts.Economy.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Economy.Items
{
    [CreateAssetMenu(menuName = "SPJ/Economy/Item", fileName = "Item")]
    public class ItemData : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public ItemType Type { get; private set; }
        [field: SerializeField] public float WeightKg { get; private set; }
        [field: SerializeField] public int BasePrice { get; private set; }
        [field: SerializeField] public float DemandWeight { get; private set; } = 1f;
        [field: SerializeField] public LocalizedString Name { get; private set; } = new();

#if UNITY_EDITOR
        public void ApplyImport(string id, ItemType type, float weightKg, int basePrice, float demandWeight, LocalizedString name)
        {
            Id = id;
            Type = type;
            WeightKg = weightKg;
            BasePrice = basePrice;
            DemandWeight = demandWeight;
            Name = name;
        }
#endif
    }
}

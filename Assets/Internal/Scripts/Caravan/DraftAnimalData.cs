using Internal.Scripts.Caravan.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Caravan
{
    [CreateAssetMenu(menuName = "SPJ/Caravan/Draft Animal", fileName = "DraftAnimal")]
    public class DraftAnimalData : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public DraftAnimalType AnimalType { get; private set; }
        [field: SerializeField] public float SpeedModPct { get; private set; }
        [field: SerializeField] public float CapacityModPct { get; private set; }
        [field: SerializeField] public float FeedPerDay { get; private set; }
        [field: SerializeField] public int Price { get; private set; }
        [field: SerializeField] public string BuyRegion { get; private set; }
        [field: SerializeField] public string SuitableBiomeBonus { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; } = new();

#if UNITY_EDITOR
        public void ApplyImport(string id, DraftAnimalType animalType, float speedModPct, float capacityModPct, float feedPerDay, int price, string buyRegion, string suitableBiomeBonus, LocalizedString name)
        {
            Id = id;
            AnimalType = animalType;
            SpeedModPct = speedModPct;
            CapacityModPct = capacityModPct;
            FeedPerDay = feedPerDay;
            Price = price;
            BuyRegion = buyRegion;
            SuitableBiomeBonus = suitableBiomeBonus;
            Name = name;
        }
#endif
    }
}

using Internal.Scripts.Caravan.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Caravan
{
    [CreateAssetMenu(menuName = "SPJ/Caravan/Cart Class", fileName = "CartClass")]
    public class CartClassData : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public CartClass ClassType { get; private set; }
        [field: SerializeField] public float SpeedKmDay { get; private set; }
        [field: SerializeField] public float Capacity { get; private set; }
        [field: SerializeField] public float Durability { get; private set; }
        [field: SerializeField] public int AnimalCount { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; } = new();
        [field: SerializeField] public LocalizedString Description { get; private set; } = new();

#if UNITY_EDITOR
        public void ApplyImport(string id, CartClass classType, float speedKmDay, float capacity, float durability, int animalCount, LocalizedString name, LocalizedString description)
        {
            Id = id;
            ClassType = classType;
            SpeedKmDay = speedKmDay;
            Capacity = capacity;
            Durability = durability;
            AnimalCount = animalCount;
            Name = name;
            Description = description;
        }
#endif
    }
}

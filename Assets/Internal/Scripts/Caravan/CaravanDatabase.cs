using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Caravan
{
    [CreateAssetMenu(menuName = "SPJ/Caravan/Database", fileName = "CaravanDatabase")]
    public class CaravanDatabase : ScriptableObject
    {
        private const string SUITABLE_COMPATIBILITY = "suitable";

        [Serializable]
        public struct CartUpgradeLevelEntry
        {
            [field: SerializeField] public CartUpgradeLevel Level { get; set; }
            [field: SerializeField] public int SortOrder { get; set; }
            [field: SerializeField] public float SpeedMult { get; set; }
            [field: SerializeField] public float CapacityMult { get; set; }
            [field: SerializeField] public float DurabilityMult { get; set; }
            [field: SerializeField] public int Price { get; set; }
            [field: SerializeField] public LocalizedString Name { get; set; }
        }

        [Serializable]
        public struct CompanionQualityEntry
        {
            [field: SerializeField] public CompanionQuality Quality { get; set; }
            [field: SerializeField] public int SortOrder { get; set; }
            [field: SerializeField] public float SuccessPct { get; set; }
            [field: SerializeField] public float PriceMultiplier { get; set; }
            [field: SerializeField] public float DailyCostMultiplier { get; set; }
            [field: SerializeField] public string Availability { get; set; }
            [field: SerializeField] public LocalizedString Name { get; set; }
        }

        [Serializable]
        public struct CaravanUpgradeEntry
        {
            [field: SerializeField] public CaravanUpgradeType Type { get; set; }
            [field: SerializeField] public int Price { get; set; }
            [field: SerializeField] public BuildingId Building { get; set; }
            [field: SerializeField] public string EffectDescription { get; set; }
            [field: SerializeField] public LocalizedString Name { get; set; }
            [field: SerializeField] public LocalizedString Description { get; set; }
        }

        [Serializable]
        public struct AnimalBiomeEntry
        {
            [field: SerializeField] public DraftAnimalType Animal { get; set; }
            [field: SerializeField] public Biome Biome { get; set; }
            [field: SerializeField] public string Compatibility { get; set; }
        }

        [Serializable]
        public struct CompanionBonusEntry
        {
            [field: SerializeField] public CompanionType Type { get; set; }
            [field: SerializeField] public CompanionQuality Quality { get; set; }
            [field: SerializeField] public string BonusKey { get; set; }
            [field: SerializeField] public float BonusValue { get; set; }
            [field: SerializeField] public LocalizedString Description { get; set; }
        }

        [field: SerializeField] public List<CartClassData> CartClasses { get; private set; } = new();
        [field: SerializeField] public List<ExtraCartData> ExtraCarts { get; private set; } = new();
        [field: SerializeField] public List<CompanionTypeData> CompanionTypes { get; private set; } = new();
        [field: SerializeField] public List<DraftAnimalData> DraftAnimals { get; private set; } = new();

        [field: SerializeField] public List<CartUpgradeLevelEntry> UpgradeLevels { get; private set; } = new();
        [field: SerializeField] public List<CompanionQualityEntry> CompanionQualities { get; private set; } = new();
        [field: SerializeField] public List<CompanionBonusEntry> CompanionBonuses { get; private set; } = new();
        [field: SerializeField] public List<CaravanUpgradeEntry> CaravanUpgrades { get; private set; } = new();
        [field: SerializeField] public List<AnimalBiomeEntry> AnimalBiomeCompatibility { get; private set; } = new();

        public CartClassData GetCartClass(CartClass classType)
        {
            return CartClasses.Find(c => c.ClassType == classType);
        }

        public ExtraCartData GetExtraCart(ExtraCartType type)
        {
            return ExtraCarts.Find(e => e.Type == type);
        }

        public CartUpgradeLevelEntry GetUpgradeLevel(CartUpgradeLevel level)
        {
            return UpgradeLevels.Find(u => u.Level == level);
        }

        public CompanionTypeData GetCompanionType(CompanionType type)
        {
            return CompanionTypes.Find(c => c.Type == type);
        }

        public CompanionQualityEntry GetCompanionQuality(CompanionQuality quality)
        {
            return CompanionQualities.Find(q => q.Quality == quality);
        }

        public CompanionBonusEntry GetCompanionBonus(CompanionType type, CompanionQuality quality)
        {
            return CompanionBonuses.Find(b => b.Type == type && b.Quality == quality);
        }

        public DraftAnimalData GetDraftAnimal(DraftAnimalType animalType)
        {
            return DraftAnimals.Find(a => a.AnimalType == animalType);
        }

        public CartClassData GetCartClassById(string classId)
        {
            if (string.IsNullOrEmpty(classId)) return null;
            return CartClasses.Find(c => c.Id == classId);
        }

        public DraftAnimalData GetDraftAnimalById(string animalId)
        {
            if (string.IsNullOrEmpty(animalId)) return null;
            return DraftAnimals.Find(a => a.Id == animalId);
        }

        public CartUpgradeLevelEntry GetUpgradeLevelById(string levelId)
        {
            if (!string.IsNullOrEmpty(levelId))
            {
                foreach (var entry in UpgradeLevels)
                {
                    if (entry.Level.ToString().Equals(levelId, StringComparison.OrdinalIgnoreCase))
                        return entry;
                }
            }
            return new CartUpgradeLevelEntry { SpeedMult = 1f, CapacityMult = 1f, DurabilityMult = 1f };
        }

        public CompanionQualityEntry GetCompanionQualityById(string qualityId)
        {
            if (!string.IsNullOrEmpty(qualityId))
            {
                foreach (var entry in CompanionQualities)
                {
                    if (entry.Quality.ToString().Equals(qualityId, StringComparison.OrdinalIgnoreCase))
                        return entry;
                }
            }
            return new CompanionQualityEntry { DailyCostMultiplier = 1f, PriceMultiplier = 1f, SuccessPct = 0f };
        }

        public CompanionTypeData GetCompanionTypeById(string typeId)
        {
            if (string.IsNullOrEmpty(typeId)) return null;
            return CompanionTypes.Find(c => string.Equals(c.Id, typeId, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsAnimalSuitable(DraftAnimalType animalType, Biome biome)
        {
            return AnimalBiomeCompatibility.Exists(e =>
                e.Animal == animalType && e.Biome == biome &&
                string.Equals(e.Compatibility, SUITABLE_COMPATIBILITY, StringComparison.OrdinalIgnoreCase));
        }

#if UNITY_EDITOR
        public void ApplyImport(
            List<CartClassData> cartClasses,
            List<ExtraCartData> extraCarts,
            List<CompanionTypeData> companionTypes,
            List<DraftAnimalData> draftAnimals,
            List<CartUpgradeLevelEntry> upgradeLevels,
            List<CompanionQualityEntry> companionQualities,
            List<CompanionBonusEntry> companionBonuses,
            List<CaravanUpgradeEntry> caravanUpgrades,
            List<AnimalBiomeEntry> animalBiomeCompatibility)
        {
            CartClasses = cartClasses ?? new List<CartClassData>();
            ExtraCarts = extraCarts ?? new List<ExtraCartData>();
            CompanionTypes = companionTypes ?? new List<CompanionTypeData>();
            DraftAnimals = draftAnimals ?? new List<DraftAnimalData>();
            UpgradeLevels = upgradeLevels ?? new List<CartUpgradeLevelEntry>();
            CompanionQualities = companionQualities ?? new List<CompanionQualityEntry>();
            CompanionBonuses = companionBonuses ?? new List<CompanionBonusEntry>();
            CaravanUpgrades = caravanUpgrades ?? new List<CaravanUpgradeEntry>();
            AnimalBiomeCompatibility = animalBiomeCompatibility ?? new List<AnimalBiomeEntry>();
        }
#endif
    }
}

using System.Linq;
using Internal.Scripts.Caravan;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using UnityEngine;

namespace Internal.Scripts.Player
{
    public sealed class OverloadCalculator
    {
        private readonly ItemWeightCalculator _weightCalculator;
        private readonly InventoryRepository _inventoryRepository;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly CaravanSpeedConfig _config;
        private readonly CaravanDatabase _caravanDatabase;

        public OverloadCalculator(
            ItemWeightCalculator weightCalculator,
            InventoryRepository inventoryRepository,
            PlayerResourceRepository resourceRepository,
            CaravanSpeedConfig config,
            CaravanDatabase caravanDatabase)
        {
            _weightCalculator = weightCalculator;
            _inventoryRepository = inventoryRepository;
            _resourceRepository = resourceRepository;
            _config = config;
            _caravanDatabase = caravanDatabase;
        }

        public float GetEffectiveCapacity()
        {
            PlayerResourceState resources = _resourceRepository.Current;

            var upgradeLevel = _caravanDatabase.GetUpgradeLevelById(resources.CartUpgradeLevelId);
            float mainCartCapacity = resources.PlayerCart.Capacity * upgradeLevel.CapacityMult;

            DraftAnimalData animal = _caravanDatabase.GetDraftAnimalById(resources.DraftAnimalId);
            if (animal != null)
                mainCartCapacity *= 1f + animal.CapacityModPct / 100f;

            float extraCapacity = resources.Carts?.Sum(c => c.Capacity) ?? 0f;
            return mainCartCapacity + extraCapacity;
        }

        public float GetOverloadCoefficient()
        {
            float capacity = GetEffectiveCapacity();
            if (capacity <= 0f)
                return 0f;

            float weight = _weightCalculator.CalculateInventoryWeight(
                _inventoryRepository.GetPlayerInventory());

            float ratio = weight / capacity;
            if (ratio <= 1f)
                return 0f;

            float overloadRange = _config.MaxOverloadRatio - 1f;
            if (overloadRange <= 0f)
                return 1f;

            return Mathf.Clamp01((ratio - 1f) / overloadRange);
        }

        public float GetSpeedModifier() => 1f - GetOverloadCoefficient() * _config.OverloadSpeedPenalty;
        public float GetWearModifier() => 1f + GetOverloadCoefficient() * _config.OverloadWearBonus;
        public float GetFoodModifier() => 1f + GetOverloadCoefficient() * _config.OverloadFoodBonus;
    }
}

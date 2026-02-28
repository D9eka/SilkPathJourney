using Internal.Scripts.Config;
using Internal.Scripts.Economy;
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

        public OverloadCalculator(
            ItemWeightCalculator weightCalculator,
            InventoryRepository inventoryRepository,
            PlayerResourceRepository resourceRepository,
            CaravanSpeedConfig config)
        {
            _weightCalculator = weightCalculator;
            _inventoryRepository = inventoryRepository;
            _resourceRepository = resourceRepository;
            _config = config;
        }

        public float GetOverloadCoefficient()
        {
            float capacity = _resourceRepository.Current.TotalCapacity;
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

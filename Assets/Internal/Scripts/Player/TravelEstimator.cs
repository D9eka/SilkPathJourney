using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.Player
{
    public readonly struct TravelEstimate
    {
        public readonly bool IsValid;
        public readonly int Days;
        public readonly bool SuppliesSufficient;

        public TravelEstimate(int days, bool suppliesSufficient)
        {
            IsValid = true;
            Days = days;
            SuppliesSufficient = suppliesSufficient;
        }
    }

    public sealed class TravelEstimator
    {
        private readonly IRoadPathFinder _pathFinder;
        private readonly IPlayerStateProvider _playerState;
        private readonly PlayerResourceRepository _resourceRepo;
        private readonly InventoryRepository _inventoryRepo;
        private readonly OverloadCalculator _overload;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly CaravanSpeedService _speedService;

        public TravelEstimator(
            IRoadPathFinder pathFinder,
            IPlayerStateProvider playerState,
            PlayerResourceRepository resourceRepo,
            InventoryRepository inventoryRepo,
            OverloadCalculator overload,
            GameBalanceConfig balanceConfig,
            CaravanSpeedService speedService)
        {
            _pathFinder = pathFinder;
            _playerState = playerState;
            _resourceRepo = resourceRepo;
            _inventoryRepo = inventoryRepo;
            _overload = overload;
            _balanceConfig = balanceConfig;
            _speedService = speedService;
        }

        public TravelEstimate Estimate(string targetNodeId)
        {
            string startNodeId = _playerState.CurrentNodeId;
            if (string.IsNullOrEmpty(startNodeId))
                startNodeId = _playerState.CurrentFromNodeId;

            if (string.IsNullOrEmpty(startNodeId))
                return default;

            RoadPath path = _pathFinder.FindPath(startNodeId, targetNodeId);
            PlayerResourceState resources = _resourceRepo.Current;

            float baseSpeed = _speedService.GetBaseSpeedKmDay(resources);
            float effectiveSpeed = baseSpeed * _balanceConfig.WorldUnitsPerKm * _overload.GetSpeedModifier();
            int days = path.EstimateDays(effectiveSpeed);
            if (days < 0)
                return default;

            int suppliesNeeded = Mathf.CeilToInt(resources.TotalFoodPerDay * days);
            int currentSupplies = InventoryStateMutator.GetItemCount(
                _inventoryRepo.GetPlayerInventory(), SuppliesItemId.Value);
            bool sufficient = currentSupplies >= suppliesNeeded;

            return new TravelEstimate(days, sufficient);
        }
    }
}

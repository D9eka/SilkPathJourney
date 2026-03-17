using System;
using Internal.Scripts.Caravan;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using UnityEngine;

namespace Internal.Scripts.Player
{
    public sealed class DailyTravelCosts : IDisposable
    {
        private const string REPAIR_KIT_UPGRADE = "repair_kit";
        private const float REPAIR_KIT_DAILY_AMOUNT = 1f;

        private readonly DayTracker _dayTracker;
        private readonly CaravanSpeedService _speedService;
        private readonly CaravanSpeedConfig _config;
        private readonly OverloadCalculator _overload;
        private readonly PlayerResourceRepository _resourceRepo;
        private readonly InventoryRepository _inventoryRepository;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly CaravanDatabase _caravanDatabase;
        private readonly CompanionCosts _companionCosts;

        public DailyTravelCosts(
            DayTracker dayTracker,
            CaravanSpeedService speedService,
            CaravanSpeedConfig config,
            OverloadCalculator overload,
            PlayerResourceRepository resourceRepo,
            InventoryRepository inventoryRepository,
            GameBalanceConfig balanceConfig,
            CaravanDatabase caravanDatabase,
            CompanionCosts companionCosts)
        {
            _dayTracker = dayTracker;
            _speedService = speedService;
            _config = config;
            _overload = overload;
            _resourceRepo = resourceRepo;
            _inventoryRepository = inventoryRepository;
            _balanceConfig = balanceConfig;
            _caravanDatabase = caravanDatabase;
            _companionCosts = companionCosts;
        }

        public void Activate()
        {
            _dayTracker.OnDayChanged += HandleDayChanged;
        }

        public void Dispose()
        {
            _dayTracker.OnDayChanged -= HandleDayChanged;
        }

        private void HandleDayChanged(int day)
        {
            SpeedModeData modeData = _config.GetModeData(_speedService.CurrentMode.Value);
            float overloadWear = _overload.GetWearModifier();
            float overloadFood = _overload.GetFoodModifier();

            int suppliesToConsume = 0;

            _resourceRepo.UpdateResources(state =>
            {
                ApplyDurabilityWear(state, modeData, overloadWear);
                suppliesToConsume = AccumulateFoodConsumption(state, modeData, overloadFood);
                ApplyDangerIncrease(state, modeData);
                _companionCosts.ProcessDailyPay(state);
                ApplyRepairKitUpgrade(state);
            });

            if (suppliesToConsume > 0)
            {
                _inventoryRepository.UpdatePlayerInventory(inv =>
                    InventoryStateMutator.RemoveItems(inv, SuppliesItemId.Value, suppliesToConsume));
            }
        }

        private void ApplyDurabilityWear(PlayerResourceState state, SpeedModeData data, float overloadMod)
        {
            float wearRate = _config.BaseDurabilityWearRate * data.WearMultiplier * overloadMod;

            state.PlayerCart.Durability = Mathf.Max(0f,
                state.PlayerCart.Durability - state.PlayerCart.MaxDurability * wearRate);

            foreach (CartState cart in state.Carts)
                cart.Durability = Mathf.Max(0f, cart.Durability - cart.MaxDurability * wearRate);
        }

        private int AccumulateFoodConsumption(PlayerResourceState state, SpeedModeData data, float overloadMod)
        {
            float baseFoodPerDay = state.TotalFoodPerDay;
            baseFoodPerDay += CalculateAnimalFeed(state);
            baseFoodPerDay += state.Companions?.Count ?? 0;

            state.Food += baseFoodPerDay * data.FoodMultiplier * overloadMod;

            int toConsume = (int)state.Food;
            state.Food -= toConsume;
            return toConsume;
        }

        private float CalculateAnimalFeed(PlayerResourceState state)
        {
            CartClassData classData = _caravanDatabase.GetCartClassById(state.CartClassId);
            DraftAnimalData animal = _caravanDatabase.GetDraftAnimalById(state.DraftAnimalId);
            if (classData == null || animal == null)
                return 0f;

            return classData.AnimalCount * animal.FeedPerDay;
        }

        private void ApplyDangerIncrease(PlayerResourceState state, SpeedModeData data)
        {
            if (data.DangerPerDay > 0f)
                state.AccumulatedDanger = Mathf.Min(
                    _balanceConfig.MaxDanger, state.AccumulatedDanger + data.DangerPerDay);
        }

        private void ApplyRepairKitUpgrade(PlayerResourceState state)
        {
            if (state.ActiveUpgrades != null && state.ActiveUpgrades.Contains(REPAIR_KIT_UPGRADE))
            {
                state.PlayerCart.Durability = Mathf.Min(
                    state.PlayerCart.MaxDurability, state.PlayerCart.Durability + REPAIR_KIT_DAILY_AMOUNT);
            }
        }
    }
}

using System;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events;
using UnityEngine;

namespace Internal.Scripts.Player
{
    public sealed class DailyTravelCosts : IDisposable
    {
        private readonly DayTracker _dayTracker;
        private readonly CaravanSpeedService _speedService;
        private readonly CaravanSpeedConfig _config;
        private readonly OverloadCalculator _overload;
        private readonly PlayerResourceRepository _resourceRepo;
        private readonly GameBalanceConfig _balanceConfig;

        public DailyTravelCosts(
            DayTracker dayTracker,
            CaravanSpeedService speedService,
            CaravanSpeedConfig config,
            OverloadCalculator overload,
            PlayerResourceRepository resourceRepo,
            GameBalanceConfig balanceConfig)
        {
            _dayTracker = dayTracker;
            _speedService = speedService;
            _config = config;
            _overload = overload;
            _resourceRepo = resourceRepo;
            _balanceConfig = balanceConfig;
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

            _resourceRepo.UpdateResources(state =>
            {
                ApplyDurabilityWear(state, modeData, overloadWear);
                ApplyFoodConsumption(state, modeData, overloadFood);
                ApplyDangerIncrease(state, modeData);
            });
        }

        private void ApplyDurabilityWear(PlayerResourceState state, SpeedModeData data, float overloadMod)
        {
            float wearRate = _config.BaseDurabilityWearRate * data.WearMultiplier * overloadMod;

            state.PlayerCart.Durability = Mathf.Max(0f,
                state.PlayerCart.Durability - state.PlayerCart.MaxDurability * wearRate);

            foreach (CartState cart in state.Carts)
                cart.Durability = Mathf.Max(0f, cart.Durability - cart.MaxDurability * wearRate);
        }

        private void ApplyFoodConsumption(PlayerResourceState state, SpeedModeData data, float overloadMod)
        {
            float baseFoodPerDay = state.PlayerCart.FoodConsumptionPerDay;
            foreach (CartState cart in state.Carts)
                baseFoodPerDay += cart.FoodConsumptionPerDay;

            state.Food = Mathf.Max(0f, state.Food - baseFoodPerDay * data.FoodMultiplier * overloadMod);
        }

        private void ApplyDangerIncrease(PlayerResourceState state, SpeedModeData data)
        {
            if (data.DangerPerDay > 0f)
                state.AccumulatedDanger = Mathf.Min(
                    _balanceConfig.MaxDanger, state.AccumulatedDanger + data.DangerPerDay);
        }
    }
}

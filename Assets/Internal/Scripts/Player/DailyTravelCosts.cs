using System;
using Internal.Scripts.Camp;
using Internal.Scripts.Caravan;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.WorldModifiers;
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
        private readonly ModifierEffectQuery _modifierQuery;
        private readonly CurrentRoadResolver _roadResolver;
        private readonly ICityNodeResolver _cityResolver;
        private readonly IPlayerStateProvider _playerState;
        private readonly CampActionService _campActionService;
        private readonly CampActionDatabase _campActionDatabase;

        public DailyTravelCosts(
            DayTracker dayTracker,
            CaravanSpeedService speedService,
            CaravanSpeedConfig config,
            OverloadCalculator overload,
            PlayerResourceRepository resourceRepo,
            InventoryRepository inventoryRepository,
            GameBalanceConfig balanceConfig,
            CaravanDatabase caravanDatabase,
            CompanionCosts companionCosts,
            ModifierEffectQuery modifierQuery,
            CurrentRoadResolver roadResolver,
            ICityNodeResolver cityResolver,
            IPlayerStateProvider playerState,
            CampActionService campActionService,
            CampActionDatabase campActionDatabase)
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
            _modifierQuery = modifierQuery;
            _roadResolver = roadResolver;
            _cityResolver = cityResolver;
            _playerState = playerState;
            _campActionService = campActionService;
            _campActionDatabase = campActionDatabase;
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

            string roadId = _roadResolver.GetCurrentRoadId();
            float suppliesMult = roadId != null ? _modifierQuery.GetRoadSuppliesMultiplier(roadId) : 1f;
            float dangerMult = roadId != null ? _modifierQuery.GetRoadDangerMultiplier(roadId) : 1f;

            string nodeId = _playerState.CurrentNodeId;
            if (!string.IsNullOrEmpty(nodeId) && _cityResolver.TryGetCityByNodeId(nodeId, out CityData city))
                dangerMult *= _modifierQuery.GetCityDangerMultiplier(city.Id);

            bool skipFood = ResolveSkipFoodConsumption();
            int suppliesToConsume = 0;

            _resourceRepo.UpdateResources(state =>
            {
                ApplyDurabilityWear(state, modeData, overloadWear);
                if (!skipFood)
                    suppliesToConsume = AccumulateFoodConsumption(state, modeData, overloadFood, suppliesMult);
                ApplyDangerIncrease(state, modeData, dangerMult);
                _companionCosts.ProcessDailyPay(state);
                ApplyRepairKitUpgrade(state);
            });

            if (suppliesToConsume > 0)
            {
                _inventoryRepository.UpdatePlayerInventory(inv =>
                    InventoryStateMutator.RemoveItems(inv, SuppliesItemId.Value, suppliesToConsume));
            }
        }

        private bool ResolveSkipFoodConsumption()
        {
            CampActionType? action = _campActionService.CurrentAction;
            if (!action.HasValue) return false;
            var data = _campActionDatabase.GetAction(action.Value);
            return data != null && data.SkipDailyFoodConsumption;
        }

        private void ApplyDurabilityWear(PlayerResourceState state, SpeedModeData data, float overloadMod)
        {
            float wearRate = _config.BaseDurabilityWearRate * data.WearMultiplier * overloadMod;

            state.PlayerCart.Durability = Mathf.Max(0f,
                state.PlayerCart.Durability - state.PlayerCart.MaxDurability * wearRate);

            foreach (CartState cart in state.Carts)
                cart.Durability = Mathf.Max(0f, cart.Durability - cart.MaxDurability * wearRate);
        }

        private int AccumulateFoodConsumption(PlayerResourceState state, SpeedModeData data, float overloadMod, float suppliesMult)
        {
            float baseFoodPerDay = state.TotalFoodPerDay;
            baseFoodPerDay += CalculateAnimalFeed(state);
            baseFoodPerDay += state.Companions?.Count ?? 0;

            state.Food += baseFoodPerDay * data.FoodMultiplier * overloadMod * suppliesMult;

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

        private void ApplyDangerIncrease(PlayerResourceState state, SpeedModeData data, float dangerMult)
        {
            if (data.DangerPerDay > 0f)
                state.AccumulatedDanger = Mathf.Min(
                    _balanceConfig.MaxDanger, state.AccumulatedDanger + data.DangerPerDay * dangerMult);
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

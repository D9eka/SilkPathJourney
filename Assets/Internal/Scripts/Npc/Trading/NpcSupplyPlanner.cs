using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Road.Path;
using Internal.Scripts.Trading;
using UnityEngine;

namespace Internal.Scripts.Npc.Trading
{
    public sealed class NpcSupplyPlanner
    {
        private readonly CityTransactionService _transactionService;
        private readonly NpcSimulationSettings _settings;
        private readonly IRoadPathFinder _pathFinder;
        private readonly InventoryRepository _inventoryRepository;
        private readonly CityTradePriceService _priceService;

        public NpcSupplyPlanner(
            CityTransactionService transactionService,
            NpcSimulationSettings settings,
            IRoadPathFinder pathFinder,
            InventoryRepository inventoryRepository,
            CityTradePriceService priceService)
        {
            _transactionService = transactionService;
            _settings = settings;
            _pathFinder = pathFinder;
            _inventoryRepository = inventoryRepository;
            _priceService = priceService;
        }

        public int EstimateSuppliesNeeded(string fromNodeId, string toNodeId, float speed)
        {
            if (string.IsNullOrEmpty(toNodeId) || speed <= 0f)
                return -1;

            RoadPath path = _pathFinder.FindPath(fromNodeId, toNodeId);
            if (path == null)
                return (_settings.ExtraSuppliesDays + _settings.NoPathFallbackDays) * _settings.SuppliesPerDay;

            int days = path.EstimateDays(speed);
            if (days < 0)
                return -1;

            return (days + _settings.ExtraSuppliesDays) * _settings.SuppliesPerDay;
        }

        public float EstimateTransportDays(string fromNodeId, string toNodeId, float speed)
        {
            if (string.IsNullOrEmpty(toNodeId) || speed <= 0f)
                return 0f;

            int suppliesNeeded = EstimateSuppliesNeeded(fromNodeId, toNodeId, speed);
            if (suppliesNeeded < 0)
                return 0f;

            return (float)suppliesNeeded / _settings.SuppliesPerDay;
        }

        public int CalculateSupplyDeficit(NpcEconomyState agent,
            string currentNodeId, string targetNodeId, float speedMetersPerDay)
        {
            int suppliesNeeded = EstimateSuppliesNeeded(currentNodeId, targetNodeId, speedMetersPerDay);
            if (suppliesNeeded < 0)
                return -1;

            int currentSupplies = InventoryStateMutator.GetItemCount(agent.Inventory, SuppliesItemId.Value);
            return Mathf.Max(0, suppliesNeeded - currentSupplies);
        }

        public int EstimateSupplyPurchaseCost(NpcEconomyState agent, string cityId,
            string currentNodeId, string targetNodeId, float speedMetersPerDay)
        {
            int deficit = CalculateSupplyDeficit(agent, currentNodeId, targetNodeId, speedMetersPerDay);
            if (deficit < 0)
                return -1;
            if (deficit == 0)
                return 0;

            int buyPrice = _priceService.GetPrice(cityId, SuppliesItemId.Value, TradePriceKind.BuyFromCity,
                applySkillBonus: false);
            if (buyPrice <= 0)
                return -1;

            return deficit * buyPrice;
        }

        public int CalculateTradeBudgetAfterSupplies(NpcEconomyState agent, string cityId,
            string currentNodeId, string targetNodeId, float speedMetersPerDay)
        {
            int supplyCost = EstimateSupplyPurchaseCost(
                agent, cityId, currentNodeId, targetNodeId, speedMetersPerDay);
            if (supplyCost < 0)
                return -1;

            int budget = Mathf.RoundToInt(agent.Money * _settings.BuyBudgetFraction);
            int reserveForSupplies = agent.Money - budget;
            if (reserveForSupplies < supplyCost)
                budget -= (supplyCost - reserveForSupplies);
            return budget;
        }

        public bool CanAffordTrip(NpcEconomyState agent, string cityId,
            string currentNodeId, string targetNodeId, float speedMetersPerDay)
        {
            int deficit = CalculateSupplyDeficit(agent, currentNodeId, targetNodeId, speedMetersPerDay);
            if (deficit < 0)
                return false;

            if (deficit == 0)
                return Mathf.RoundToInt(agent.Money * _settings.BuyBudgetFraction) > 0;

            CityInventoryState cityState = _inventoryRepository.GetCityInventory(cityId);
            ItemStackState cityStack = cityState?.Inventory?.Items?.Find(s => s.ItemId == SuppliesItemId.Value);
            int available = cityStack?.Count ?? 0;

            int buyPrice = _priceService.GetPrice(cityId, SuppliesItemId.Value, TradePriceKind.BuyFromCity,
                applySkillBonus: false);
            if (buyPrice <= 0)
                return false;

            int maxByBudget = agent.Money / buyPrice;
            int canBuy = Mathf.Min(deficit, Mathf.Min(available, maxByBudget));
            if (canBuy < deficit)
                return false;

            int supplyCost = canBuy * buyPrice;
            int budget = Mathf.RoundToInt(agent.Money * _settings.BuyBudgetFraction);
            int reserve = agent.Money - budget;
            if (reserve < supplyCost)
                budget -= (supplyCost - reserve);
            return budget > 0;
        }

        public (int count, int cost) BuySupplies(NpcEconomyState agent, string cityId,
            string currentNodeId, string nextDestNodeId, float speedMetersPerDay)
        {
            int suppliesNeeded = EstimateSuppliesNeeded(currentNodeId, nextDestNodeId, speedMetersPerDay);
            if (suppliesNeeded < 0)
                return (0, 0);

            int currentSupplies = InventoryStateMutator.GetItemCount(agent.Inventory, SuppliesItemId.Value);
            int deficit = suppliesNeeded - currentSupplies;
            if (deficit <= 0)
                return (0, 0);

            return _transactionService.BuyFromCity(agent, cityId, SuppliesItemId.Value, deficit);
        }
    }
}

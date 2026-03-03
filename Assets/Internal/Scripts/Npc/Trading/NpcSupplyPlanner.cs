using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
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
            if (!path.IsValid)
                return -1;

            int days = Mathf.CeilToInt(path.TotalLengthMeters / speed);
            return (days + _settings.ExtraSuppliesDays) * _settings.SuppliesPerDay;
        }

        public bool CanAffordTrip(NpcEconomyState agent, string cityId,
            string currentNodeId, string targetNodeId, float speedMetersPerDay)
        {
            int suppliesNeeded = EstimateSuppliesNeeded(currentNodeId, targetNodeId, speedMetersPerDay);
            if (suppliesNeeded < 0)
                return false;

            int currentSupplies = InventoryStateMutator.GetItemCount(agent.Inventory, SuppliesItemId.Value);
            if (currentSupplies >= suppliesNeeded)
                return true;

            CityInventoryState cityState = _inventoryRepository.GetCityInventory(cityId);
            ItemStackState cityStack = cityState?.Inventory?.Items?.Find(s => s.ItemId == SuppliesItemId.Value);
            int available = cityStack?.Count ?? 0;

            int buyPrice = _priceService.GetPrice(cityId, SuppliesItemId.Value, TradePriceKind.BuyFromCity);
            if (buyPrice <= 0)
                return currentSupplies >= suppliesNeeded;

            int maxByBudget = agent.Money / buyPrice;
            int canBuy = Mathf.Min(available, maxByBudget);
            return currentSupplies + canBuy >= suppliesNeeded;
        }

        public void BuySupplies(NpcEconomyState agent, string cityId,
            string currentNodeId, string nextDestNodeId, float speedMetersPerDay)
        {
            int suppliesNeeded = EstimateSuppliesNeeded(currentNodeId, nextDestNodeId, speedMetersPerDay);
            if (suppliesNeeded < 0)
                return;

            int currentSupplies = InventoryStateMutator.GetItemCount(agent.Inventory, SuppliesItemId.Value);
            int deficit = suppliesNeeded - currentSupplies;
            if (deficit <= 0)
                return;

            _transactionService.BuyFromCity(agent, cityId, SuppliesItemId.Value, deficit);
        }
    }
}

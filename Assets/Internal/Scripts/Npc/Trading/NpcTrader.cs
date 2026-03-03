using System.Collections.Generic;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Trading;
using Internal.Scripts.Utils;
using UnityEngine;

namespace Internal.Scripts.Npc.Trading
{
    public sealed class NpcTrader
    {
        private readonly CityTransactionService _transactionService;
        private readonly NpcSupplyPlanner _supplyPlanner;
        private readonly InventoryRepository _inventoryRepository;
        private readonly ItemCatalog _itemCatalog;
        private readonly ItemWeightCalculator _itemWeightCalculator;
        private readonly NpcSimulationSettings _settings;

        public NpcTrader(
            CityTransactionService transactionService,
            NpcSupplyPlanner supplyPlanner,
            InventoryRepository inventoryRepository,
            ItemCatalog itemCatalog,
            ItemWeightCalculator itemWeightCalculator,
            NpcSimulationSettings settings)
        {
            _transactionService = transactionService;
            _supplyPlanner = supplyPlanner;
            _inventoryRepository = inventoryRepository;
            _itemCatalog = itemCatalog;
            _itemWeightCalculator = itemWeightCalculator;
            _settings = settings;
        }

        public void ExecuteTrade(NpcEconomyState agent, string cityId,
            string currentNodeId, string nextDestNodeId, float speedMetersPerDay)
        {
            SellGoods(agent, cityId);
            _supplyPlanner.BuySupplies(agent, cityId, currentNodeId, nextDestNodeId, speedMetersPerDay);
            BuyRandomGoods(agent, cityId);
        }

        public void SellGoods(NpcEconomyState agent, string cityId)
        {
            if (agent.Inventory?.Items == null || agent.Inventory.Items.Count == 0)
                return;

            List<ItemStackState> toSell = new(agent.Inventory.Items);
            toSell.RemoveAll(s => s.ItemId == SuppliesItemId.Value);

            if (toSell.Count == 0)
                return;

            _transactionService.SellToCity(agent, cityId, toSell);
        }

        public void BuyRandomGoods(NpcEconomyState agent, string cityId)
        {
            int budget = Mathf.RoundToInt(agent.Money * _settings.BuyBudgetFraction);
            if (budget <= 0)
                return;

            float remainingCapacity = agent.CapacityKg -
                _itemWeightCalculator.CalculateInventoryWeight(agent.Inventory);
            if (remainingCapacity <= 0f)
                return;

            CityInventoryState cityState = _inventoryRepository.GetCityInventory(cityId);
            if (cityState?.Inventory?.Items == null || cityState.Inventory.Items.Count == 0)
                return;

            List<ItemStackState> available = GetAvailableItems(cityState);

            int typesBought = 0;
            foreach (ItemStackState stock in available)
            {
                if (typesBought >= _settings.MaxBuyItemTypes || budget <= 0 || remainingCapacity <= 0f)
                    break;

                int buyPrice = _transactionService.GetBuyPrice(cityId, stock.ItemId);
                if (buyPrice <= 0)
                    continue;

                float itemWeight = _itemCatalog.GetItemWeight(stock.ItemId);
                int maxByBudget = budget / buyPrice;
                int maxByCapacity = itemWeight > 0f
                    ? Mathf.FloorToInt(remainingCapacity / itemWeight)
                    : int.MaxValue;

                int desiredCount = Mathf.Min(maxByBudget, maxByCapacity);
                if (desiredCount <= 0)
                    continue;

                var (count, cost) = _transactionService.BuyFromCity(agent, cityId, stock.ItemId, desiredCount);
                if (count <= 0)
                    continue;

                budget -= cost;
                remainingCapacity -= itemWeight * count;
                typesBought++;
            }
        }

        private List<ItemStackState> GetAvailableItems(CityInventoryState cityState)
        {
            List<ItemStackState> available = new();
            foreach (ItemStackState stack in cityState.Inventory.Items)
            {
                if (stack != null && stack.Count > 0 && stack.ItemId != SuppliesItemId.Value)
                    available.Add(stack);
            }

            available.Shuffle();
            return available;
        }
    }
}

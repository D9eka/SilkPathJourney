using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Events;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Trading;
using UnityEngine;

namespace Internal.Scripts.Npc.Trading
{
    public sealed class NpcTrader
    {
        public readonly struct TradeExecutionStats
        {
            public TradeExecutionStats(
                int suppliesBoughtUnits,
                int goodsBoughtUnits,
                int goodsSoldUnits,
                int moneySpentOnSupplies,
                int moneySpentOnGoods,
                int moneyEarnedFromGoods)
            {
                SuppliesBoughtUnits = suppliesBoughtUnits;
                GoodsBoughtUnits = goodsBoughtUnits;
                GoodsSoldUnits = goodsSoldUnits;
                MoneySpentOnSupplies = moneySpentOnSupplies;
                MoneySpentOnGoods = moneySpentOnGoods;
                MoneyEarnedFromGoods = moneyEarnedFromGoods;
            }

            public int SuppliesBoughtUnits { get; }
            public int GoodsBoughtUnits { get; }
            public int GoodsSoldUnits { get; }
            public int MoneySpentOnSupplies { get; }
            public int MoneySpentOnGoods { get; }
            public int MoneyEarnedFromGoods { get; }

            public int BoughtUnits => SuppliesBoughtUnits + GoodsBoughtUnits;
            public int SoldUnits => GoodsSoldUnits;
            public int MoneySpent => MoneySpentOnSupplies + MoneySpentOnGoods;
            public int MoneyEarned => MoneyEarnedFromGoods;

            public static TradeExecutionStats operator +(TradeExecutionStats left, TradeExecutionStats right)
            {
                return new TradeExecutionStats(
                    left.SuppliesBoughtUnits + right.SuppliesBoughtUnits,
                    left.GoodsBoughtUnits + right.GoodsBoughtUnits,
                    left.GoodsSoldUnits + right.GoodsSoldUnits,
                    left.MoneySpentOnSupplies + right.MoneySpentOnSupplies,
                    left.MoneySpentOnGoods + right.MoneySpentOnGoods,
                    left.MoneyEarnedFromGoods + right.MoneyEarnedFromGoods);
            }
        }

        private readonly struct BuyCandidate
        {
            public BuyCandidate(string itemId, float totalExpectedProfit, float grossProfitPerUnit,
                int buyPrice, float weightKg, int availableStock)
            {
                ItemId = itemId;
                TotalExpectedProfit = totalExpectedProfit;
                GrossProfitPerUnit = grossProfitPerUnit;
                BuyPrice = buyPrice;
                WeightKg = weightKg;
                AvailableStock = availableStock;
            }

            public string ItemId { get; }
            public float TotalExpectedProfit { get; }
            public float GrossProfitPerUnit { get; }
            public int BuyPrice { get; }
            public float WeightKg { get; }
            public int AvailableStock { get; }
        }

        private readonly CityTransactionService _transactionService;
        private readonly NpcSupplyPlanner _supplyPlanner;
        private readonly InventoryRepository _inventoryRepository;
        private readonly ItemCatalog _itemCatalog;
        private readonly ItemWeightCalculator _itemWeightCalculator;
        private readonly NpcSimulationSettings _settings;
        private readonly NpcSellEstimator _sellEstimator;
        private readonly EconomyDatabase _economyDatabase;
        private readonly DayTracker _dayTracker;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly CultureDistanceService _cultureDistance;
        private readonly NpcSellService _sellService;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly NpcGuildTradeService _guildTradeService;

        public NpcTrader(
            CityTransactionService transactionService,
            NpcSupplyPlanner supplyPlanner,
            InventoryRepository inventoryRepository,
            ItemCatalog itemCatalog,
            ItemWeightCalculator itemWeightCalculator,
            NpcSimulationSettings settings,
            NpcSellEstimator sellEstimator,
            EconomyDatabase economyDatabase,
            DayTracker dayTracker,
            ICityNodeResolver cityNodeResolver,
            CultureDistanceService cultureDistance,
            NpcSellService sellService,
            GameBalanceConfig balanceConfig,
            NpcGuildTradeService guildTradeService)
        {
            _transactionService = transactionService;
            _supplyPlanner = supplyPlanner;
            _inventoryRepository = inventoryRepository;
            _itemCatalog = itemCatalog;
            _itemWeightCalculator = itemWeightCalculator;
            _settings = settings;
            _sellEstimator = sellEstimator;
            _economyDatabase = economyDatabase;
            _dayTracker = dayTracker;
            _cityNodeResolver = cityNodeResolver;
            _cultureDistance = cultureDistance;
            _sellService = sellService;
            _balanceConfig = balanceConfig;
            _guildTradeService = guildTradeService;
        }

        private TradeExecutionStats ExecuteTrade(NpcEconomyState agent, string cityId,
            string currentNodeId, string nextDestNodeId, float speedMetersPerDay)
        {
            TradeExecutionStats sellPhase = HandleArrivalSellPhase(agent, cityId);
            TradeExecutionStats buyPhase = HandleDepartureBuyPhase(
                agent, cityId, currentNodeId, nextDestNodeId, speedMetersPerDay);
            return sellPhase + buyPhase;
        }

        public TradeExecutionStats HandleArrivalSellPhase(NpcEconomyState agent, string cityId)
        {
            return _sellService.HandleArrivalSellPhase(agent, cityId);
        }

        public TradeExecutionStats HandleDepartureBuyPhase(NpcEconomyState agent, string cityId,
            string currentNodeId, string nextDestNodeId, float speedMetersPerDay)
        {
            var (suppliesBought, suppliesCost) = _supplyPlanner.BuySupplies(
                agent, cityId, currentNodeId, nextDestNodeId, speedMetersPerDay);
            var (goodsBought, goodsCost) = SmartBuyGoods(
                agent, cityId, currentNodeId, nextDestNodeId, speedMetersPerDay);
            _guildTradeService.HandleGuildCredit(agent, cityId);

            return new TradeExecutionStats(
                suppliesBought,
                goodsBought,
                0,
                suppliesCost,
                goodsCost,
                0);
        }

        public bool HasTradeCargo(NpcEconomyState agent)
        {
            if (agent?.Inventory?.Items == null)
                return false;

            foreach (ItemStackState stack in agent.Inventory.Items)
            {
                if (stack == null || stack.Count <= 0)
                    continue;
                if (stack.ItemId != SuppliesItemId.Value)
                    return true;
            }

            return false;
        }

        public float EstimateHeldCargoTradePotential(NpcEconomyState agent, string currentCityId,
            string currentNodeId, string targetCityId, string targetNodeId, float speedMetersPerDay)
        {
            float total = 0f;
            if (agent?.Inventory?.Items == null)
                return total;

            float transportDays = _supplyPlanner.EstimateTransportDays(currentNodeId, targetNodeId, speedMetersPerDay);
            int supplyCost = _supplyPlanner.EstimateSupplyPurchaseCost(
                agent, currentCityId, currentNodeId, targetNodeId, speedMetersPerDay);
            float routeCost = transportDays * _settings.TransportCostPerDay + Mathf.Max(0, supplyCost);

            foreach (ItemStackState stack in agent.Inventory.Items)
            {
                if (stack == null || stack.Count <= 0 || stack.ItemId == SuppliesItemId.Value)
                    continue;

                int currentSellPrice = _transactionService.GetSellPrice(currentCityId, stack.ItemId);
                float targetSellPrice = _sellEstimator.EstimateSellPrice(agent, targetCityId, stack.ItemId);
                float marginalProfit = (targetSellPrice - currentSellPrice) * stack.Count;
                if (marginalProfit > 0f)
                    total += marginalProfit;
            }

            return Mathf.Max(0f, total - routeCost);
        }

        public float EstimateBootstrapTradePotential(NpcEconomyState agent, string cityId,
            string currentNodeId, string nextDestNodeId, float speedMetersPerDay)
        {
            if (!TryBuildBuyCandidates(agent, cityId, currentNodeId, nextDestNodeId,
                    speedMetersPerDay, out List<BuyCandidate> candidates,
                    out int budget, out float remainingCapacity, out float routeTransportCost, out _))
            {
                return 0f;
            }

            float plannedGrossProfit = EstimatePlannedGrossProfit(
                candidates, budget, remainingCapacity, _settings.MaxBuyItemTypes, _settings.BudgetShares);
            return Mathf.Max(0f, plannedGrossProfit - routeTransportCost);
        }

        private (int boughtUnits, int moneySpent) SmartBuyGoods(NpcEconomyState agent, string cityId,
            string currentNodeId, string nextDestNodeId, float speedMetersPerDay)
        {
            if (!TryBuildBuyCandidates(agent, cityId, currentNodeId, nextDestNodeId,
                    speedMetersPerDay, out List<BuyCandidate> candidates,
                    out int budget, out float remainingCapacity, out float routeTransportCost, out string debugInfo))
            {
                if (!string.IsNullOrEmpty(debugInfo))
                    Debug.Log(debugInfo);
                return (0, 0);
            }

            float plannedGrossProfit = EstimatePlannedGrossProfit(
                candidates, budget, remainingCapacity, _settings.MaxBuyItemTypes, _settings.BudgetShares);
            if (plannedGrossProfit <= routeTransportCost)
            {
                if (!string.IsNullOrEmpty(debugInfo))
                    Debug.Log(debugInfo);
                return (0, 0);
            }

            int currentDay = _dayTracker.CurrentDay;
            CityData currentCityForPurchase = _economyDatabase.Cities.Find(c => c.Id == cityId);
            CultureId originCultureForPurchase = currentCityForPurchase?.PrimaryCulture ?? CultureId.None;

            var (boughtUnits, moneySpent) = ExecuteBuyOrders(
                agent, cityId, candidates, budget, remainingCapacity, currentDay, originCultureForPurchase);

            return (boughtUnits, moneySpent);
        }

        private (int boughtUnits, int moneySpent) ExecuteBuyOrders(
            NpcEconomyState agent, string cityId, List<BuyCandidate> candidates,
            int budget, float remainingCapacity, int currentDay, CultureId originCulture)
        {
            int take = Mathf.Min(candidates.Count, _settings.MaxBuyItemTypes);
            int totalBoughtUnits = 0;
            int totalSpent = 0;
            CityInventoryState cityStateNow = _inventoryRepository.GetCityInventory(cityId);

            for (int i = 0; i < take; i++)
            {
                if (budget <= 0 || remainingCapacity <= 0f)
                    break;

                BuyCandidate candidate = candidates[i];
                float share = i < _settings.BudgetShares.Length ? _settings.BudgetShares[i] : _settings.BudgetShares[_settings.BudgetShares.Length - 1];
                int itemBudget = Mathf.RoundToInt(budget * share);
                int maxByBudget = itemBudget / candidate.BuyPrice;
                int maxByCapacity = candidate.WeightKg > 0f
                    ? Mathf.FloorToInt(remainingCapacity / candidate.WeightKg)
                    : int.MaxValue;

                ItemStackState cityStack = cityStateNow?.Inventory?.Items?.Find(s => s.ItemId == candidate.ItemId);
                int maxByStock = cityStack?.Count ?? 0;

                int desired = Mathf.Min(maxByBudget, Mathf.Min(maxByCapacity, maxByStock));
                if (desired <= 0)
                    continue;

                var (count, cost) = _transactionService.BuyFromCity(agent, cityId, candidate.ItemId, desired);
                if (count <= 0)
                    continue;

                agent.Purchases.RemoveAll(p => p.ItemId == candidate.ItemId);
                agent.Purchases.Add(new PurchaseRecord
                {
                    ItemId = candidate.ItemId,
                    PricePerUnit = candidate.BuyPrice,
                    DayBought = currentDay,
                    OriginCulture = originCulture
                });

                budget -= cost;
                remainingCapacity -= candidate.WeightKg * count;
                totalBoughtUnits += count;
                totalSpent += cost;

                Debug.Log($"[NpcTrader] {agent.Name} bought {count}x{candidate.ItemId} for {cost}g (totalProfit={candidate.TotalExpectedProfit:F0})");
            }

            return (totalBoughtUnits, totalSpent);
        }

        private bool TryBuildBuyCandidates(NpcEconomyState agent, string cityId,
            string currentNodeId, string nextDestNodeId, float speedMetersPerDay,
            out List<BuyCandidate> candidates, out int budget, out float remainingCapacity,
            out float routeTransportCost, out string debugInfo)
        {
            candidates = new List<BuyCandidate>();
            budget = _supplyPlanner.CalculateTradeBudgetAfterSupplies(
                agent, cityId, currentNodeId, nextDestNodeId, speedMetersPerDay);
            remainingCapacity = agent.CapacityKg -
                _itemWeightCalculator.CalculateInventoryWeight(agent.Inventory);
            routeTransportCost = 0f;
            debugInfo = null;

            if (budget <= 0 || remainingCapacity <= 0f)
                return false;

            CityInventoryState cityState = _inventoryRepository.GetCityInventory(cityId);
            if (cityState?.Inventory?.Items == null || cityState.Inventory.Items.Count == 0)
                return false;

            NpcArchetypeDefinition archetype = _settings.GetArchetypeDefinition(agent.Archetype);
            float transportDays = _supplyPlanner.EstimateTransportDays(currentNodeId, nextDestNodeId, speedMetersPerDay);
            routeTransportCost = transportDays * _settings.TransportCostPerDay;
            string destCityId = _cityNodeResolver.TryGetCityByNodeId(nextDestNodeId, out CityData destCity)
                ? destCity.Id
                : nextDestNodeId;

            CityData currentCity = _economyDatabase.Cities.Find(c => c.Id == cityId);
            CultureId currentCulture = currentCity?.PrimaryCulture ?? CultureId.None;
            CultureId destCulture = destCity?.PrimaryCulture ?? CultureId.None;

            int debugSkipped = 0;
            var debugSb = new System.Text.StringBuilder();

            foreach (ItemStackState stock in cityState.Inventory.Items)
            {
                if (stock == null || stock.Count <= 0 || stock.ItemId == SuppliesItemId.Value)
                    continue;

                ItemData item = _itemCatalog.GetItem(stock.ItemId);
                if (item == null)
                    continue;

                if (!FilterCandidateItem(item, archetype, agent))
                    continue;

                BuyCandidate? scored = ScoreCandidateItem(
                    stock, item, cityId, destCityId, currentCulture, destCulture,
                    agent, budget, remainingCapacity, routeTransportCost, archetype,
                    ref debugSkipped, debugSb);

                if (scored.HasValue)
                    candidates.Add(scored.Value);
            }

            if (candidates.Count == 0 && debugSkipped > 0)
                debugInfo = $"[SmartBuy] {agent.Name} at {cityId}->{destCityId}: 0 candidates, {debugSkipped} unprofitable.{debugSb}";

            return candidates.Count > 0;
        }

        private static bool FilterCandidateItem(ItemData item, NpcArchetypeDefinition archetype, NpcEconomyState agent)
        {
            if (archetype.PreferredCategories != null && archetype.PreferredCategories.Length > 0)
            {
                bool preferred = false;
                foreach (ItemType cat in archetype.PreferredCategories)
                {
                    if (cat == item.Type)
                    {
                        preferred = true;
                        break;
                    }
                }
                if (!preferred)
                    return false;
            }

            if (archetype.MaxItemWeight > 0f && item.WeightKg > archetype.MaxItemWeight)
                return false;
            if (archetype.MinItemPrice > 0f && item.BasePrice < archetype.MinItemPrice)
                return false;
            if (agent.InDebt && (item.Type == ItemType.Luxury || item.Type == ItemType.Exotic))
                return false;

            return true;
        }

        private BuyCandidate? ScoreCandidateItem(
            ItemStackState stock, ItemData item, string cityId, string destCityId,
            CultureId currentCulture, CultureId destCulture,
            NpcEconomyState agent, int budget, float remainingCapacity, float routeTransportCost,
            NpcArchetypeDefinition archetype, ref int debugSkipped, System.Text.StringBuilder debugSb)
        {
            int buyPrice = _transactionService.GetBuyPrice(cityId, stock.ItemId);
            if (buyPrice <= 0 || item.WeightKg <= 0f)
                return null;
            if (buyPrice < _settings.MinBuyItemPrice)
                return null;

            int sellEstimate = _sellEstimator.EstimateSellPrice(agent, destCityId, stock.ItemId);
            if (currentCulture != CultureId.None && destCulture != CultureId.None)
            {
                float exoticityMult = _cultureDistance.GetExoticityMultiplier(
                    currentCulture, destCulture, _balanceConfig.ExoticityPerStep);
                sellEstimate = Mathf.RoundToInt(sellEstimate * exoticityMult);
            }

            float grossProfitPerUnit = sellEstimate - buyPrice;
            float expectedProfitRatio = buyPrice > 0 ? sellEstimate / (float)buyPrice : 0f;

            if (expectedProfitRatio < archetype.MinProfitThreshold)
            {
                debugSkipped++;
                if (debugSkipped <= 3)
                    debugSb.Append($" {stock.ItemId}(buy={buyPrice},sell={sellEstimate},ratio={expectedProfitRatio:F2},need={archetype.MinProfitThreshold:F2})");
                return null;
            }

            if (grossProfitPerUnit <= 0f)
            {
                debugSkipped++;
                if (debugSkipped <= 3)
                    debugSb.Append($" {stock.ItemId}(buy={buyPrice},sell={sellEstimate},tripCost={routeTransportCost:F0},gross={grossProfitPerUnit:F0})");
                return null;
            }

            int maxAffordable = Mathf.Min(budget / buyPrice, Mathf.FloorToInt(remainingCapacity / item.WeightKg));
            maxAffordable = Mathf.Min(maxAffordable, stock.Count);
            float totalExpectedProfit = grossProfitPerUnit * maxAffordable;
            if (totalExpectedProfit <= 0f)
                return null;

            return new BuyCandidate(stock.ItemId, totalExpectedProfit, grossProfitPerUnit, buyPrice, item.WeightKg, stock.Count);
        }

        private static float EstimatePlannedGrossProfit(
            List<BuyCandidate> candidates, int budget, float remainingCapacity, int maxItemTypes,
            float[] budgetShares)
        {
            candidates.Sort((a, b) => b.TotalExpectedProfit.CompareTo(a.TotalExpectedProfit));

            int take = Mathf.Min(candidates.Count, maxItemTypes);

            float totalGrossProfit = 0f;
            for (int i = 0; i < take; i++)
            {
                if (budget <= 0 || remainingCapacity <= 0f)
                    break;

                BuyCandidate candidate = candidates[i];
                float share = i < budgetShares.Length ? budgetShares[i] : budgetShares[budgetShares.Length - 1];
                int itemBudget = Mathf.RoundToInt(budget * share);
                int maxByBudget = itemBudget / candidate.BuyPrice;
                int maxByCapacity = candidate.WeightKg > 0f
                    ? Mathf.FloorToInt(remainingCapacity / candidate.WeightKg)
                    : int.MaxValue;

                int desired = Mathf.Min(maxByBudget, Mathf.Min(maxByCapacity, candidate.AvailableStock));
                if (desired <= 0)
                    continue;

                totalGrossProfit += candidate.GrossProfitPerUnit * desired;
                budget -= candidate.BuyPrice * desired;
                remainingCapacity -= candidate.WeightKg * desired;
            }

            return totalGrossProfit;
        }

    }
}

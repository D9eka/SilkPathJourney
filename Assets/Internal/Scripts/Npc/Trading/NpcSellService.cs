using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Events;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Trading;

namespace Internal.Scripts.Npc.Trading
{
    public sealed class NpcSellService
    {
        private readonly CityTransactionService _transactionService;
        private readonly NpcSimulationSettings _settings;
        private readonly ItemCatalog _itemCatalog;
        private readonly EconomyDatabase _economyDatabase;
        private readonly DayTracker _dayTracker;

        public NpcSellService(
            CityTransactionService transactionService,
            NpcSimulationSettings settings,
            ItemCatalog itemCatalog,
            EconomyDatabase economyDatabase,
            DayTracker dayTracker)
        {
            _transactionService = transactionService;
            _settings = settings;
            _itemCatalog = itemCatalog;
            _economyDatabase = economyDatabase;
            _dayTracker = dayTracker;
        }

        public NpcTrader.TradeExecutionStats HandleArrivalSellPhase(NpcEconomyState agent, string cityId)
        {
            var (soldUnits, moneyEarned) = SmartSellGoods(agent, cityId);
            return new NpcTrader.TradeExecutionStats(0, 0, soldUnits, 0, 0, moneyEarned);
        }

        private (int soldUnits, int moneyEarned) SmartSellGoods(NpcEconomyState agent, string cityId)
        {
            if (agent.Inventory?.Items == null || agent.Inventory.Items.Count == 0)
                return (0, 0);

            NpcArchetypeDefinition archetype = _settings.GetArchetypeDefinition(agent.Archetype);
            float holdThreshold = GetHoldThreshold(agent.Experience);
            int currentDay = _dayTracker.CurrentDay;

            List<ItemStackState> toProcess = new(agent.Inventory.Items);
            toProcess.RemoveAll(s => s.ItemId == SuppliesItemId.Value);

            List<ItemStackState> toSell = new();
            foreach (ItemStackState stack in toProcess)
            {
                if (ShouldSell(agent, cityId, stack, archetype, holdThreshold, currentDay))
                    toSell.Add(stack);
            }

            if (toSell.Count == 0)
                return (0, 0);

            var (soldUnits, received) = _transactionService.SellToCity(agent, cityId, toSell);

            foreach (ItemStackState stack in toSell)
                agent.Purchases.RemoveAll(p => p.ItemId == stack.ItemId);

            return (soldUnits, received);
        }

        private bool ShouldSell(NpcEconomyState agent, string cityId, ItemStackState stack,
            NpcArchetypeDefinition archetype, float holdThreshold, int currentDay)
        {
            PurchaseRecord? record = FindPurchaseRecord(agent, stack.ItemId);
            if (record.HasValue && currentDay - record.Value.DayBought > _settings.ForceSellDays)
                return true;

            if (agent.Money < _settings.ForceSellMoneyThreshold)
                return true;

            float itemWeight = _itemCatalog.GetItemWeight(stack.ItemId) * stack.Count;
            if (agent.CapacityKg > 0f && itemWeight > agent.CapacityKg * _settings.ForceSellCapacityFraction)
                return true;

            int sellPrice = _transactionService.GetSellPrice(cityId, stack.ItemId);
            if (record.HasValue && record.Value.PricePerUnit > 0)
            {
                float ratio = (float)sellPrice / record.Value.PricePerUnit;
                if (ratio >= archetype.MinProfitThreshold)
                    return true;
                if (ratio < 1f)
                    return false;
            }

            float sellCoef = GetSellCoefForItem(cityId, stack.ItemId);
            if (sellCoef < holdThreshold)
                return false;

            return true;
        }

        private float GetSellCoefForItem(string cityId, string itemId)
        {
            CityData city = _economyDatabase.Cities.Find(c => c.Id == cityId);
            if (city == null)
                return 1f;

            CityTypeData cityType = _economyDatabase.GetCityType(city.Type);
            if (cityType == null)
                return 1f;

            ItemData item = _itemCatalog.GetItem(itemId);
            if (item == null)
                return 1f;

            foreach (CityTypeData.CategoryCoef coef in cityType.CategoryCoefs)
            {
                if (coef.Category == item.Type)
                    return coef.SellCoef;
            }

            return 1f;
        }

        private float GetHoldThreshold(NpcExperienceLevel experience) => experience switch
        {
            NpcExperienceLevel.Novice => _settings.HoldThresholdNovice,
            NpcExperienceLevel.Experienced => _settings.HoldThresholdExperienced,
            _ => _settings.HoldThresholdMaster
        };

        private static PurchaseRecord? FindPurchaseRecord(NpcEconomyState agent, string itemId)
        {
            foreach (PurchaseRecord record in agent.Purchases)
            {
                if (record.ItemId == itemId)
                    return record;
            }

            return null;
        }
    }
}

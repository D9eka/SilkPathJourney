using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Data;
using UnityEngine;

namespace Internal.Scripts.Trading
{
    public sealed class CityTransactionService
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly CityTradePriceService _priceService;
        private readonly EconomyDatabase _economyDatabase;
        private readonly ItemCatalog _itemCatalog;
        private readonly GuildSettings _guildSettings;

        public CityTransactionService(
            InventoryRepository inventoryRepository,
            CityTradePriceService priceService,
            EconomyDatabase economyDatabase,
            ItemCatalog itemCatalog,
            GuildSettings guildSettings)
        {
            _inventoryRepository = inventoryRepository;
            _priceService = priceService;
            _economyDatabase = economyDatabase;
            _itemCatalog = itemCatalog;
            _guildSettings = guildSettings;
        }

        public (int count, int cost) BuyFromCity(
            NpcEconomyState buyer, string cityId, string itemId, int desiredCount)
        {
            CityInventoryState cityState = _inventoryRepository.GetCityInventory(cityId);
            ItemStackState cityStack = cityState?.Inventory?.Items?.Find(s => s.ItemId == itemId);
            int available = cityStack?.Count ?? 0;
            if (available <= 0)
                return (0, 0);

            int buyPrice = _priceService.GetPrice(cityId, itemId, TradePriceKind.BuyFromCity,
                applySkillBonus: false);
            if (buyPrice <= 0)
                return (0, 0);

            int maxByBudget = buyer.Money / buyPrice;
            int count = Mathf.Min(desiredCount, Mathf.Min(available, maxByBudget));
            if (count <= 0)
                return (0, 0);

            int cost = buyPrice * count;

            int capturedCount = count;
            int capturedCost = cost;
            _inventoryRepository.UpdateCityInventory(cityId, cityInv =>
            {
                InventoryStateMutator.RemoveItems(cityInv, itemId, capturedCount);
                cityInv.Money += capturedCost;
            });

            InventoryStateMutator.AddItems(buyer.Inventory, itemId, count);
            buyer.Money -= cost;
            return (count, cost);
        }

        public (int unitsSold, int moneyReceived) SellToCity(
            NpcEconomyState seller, string cityId, List<ItemStackState> items)
        {
            if (items == null || items.Count == 0)
                return (0, 0);

            int sellTotal = PriceItems(cityId, items, out var priced);
            if (sellTotal <= 0)
                return (0, 0);

            CityInventoryState cityState = _inventoryRepository.GetCityInventory(cityId);
            int cityMoney = cityState?.Inventory?.Money ?? 0;

            int totalReceived = SelectAffordableItems(priced, cityMoney, sellTotal, out var soldItems);
            if (soldItems.Count == 0)
                return (0, 0);

            int capturedTotal = totalReceived;
            _inventoryRepository.UpdateCityInventory(cityId, cityInv =>
            {
                foreach ((string itemId, int count) in soldItems)
                    InventoryStateMutator.AddItems(cityInv, itemId, count);

                cityInv.Money -= Mathf.Min(cityInv.Money, capturedTotal);
            });

            foreach ((string itemId, int count) in soldItems)
                InventoryStateMutator.RemoveItems(seller.Inventory, itemId, count);

            seller.Money += totalReceived;

            ApplyGuildTithe(seller, cityId, soldItems);

            int unitsSold = 0;
            foreach ((string _, int count) in soldItems)
                unitsSold += count;

            return (unitsSold, totalReceived);
        }

        private void ApplyGuildTithe(NpcEconomyState seller, string cityId, List<(string itemId, int count)> soldItems)
        {
            if (!HasGuildBuilding(cityId))
                return;

            int totalTithe = 0;
            foreach ((string itemId, int count) in soldItems)
            {
                ItemData item = _itemCatalog.GetItem(itemId);
                if (item == null) continue;

                float tithePct = item.Type switch
                {
                    ItemType.Craft => _guildSettings.TitheCraftPct,
                    ItemType.Luxury => _guildSettings.TitheLuxuryPct,
                    ItemType.Exotic => _guildSettings.TitheExoticPct,
                    _ => 0f
                };

                if (tithePct <= 0f) continue;

                int unitPrice = _priceService.GetPrice(cityId, itemId, TradePriceKind.SellToCity, applySkillBonus: false);
                int tithe = Mathf.RoundToInt(unitPrice * count * tithePct);
                totalTithe += tithe;
            }

            if (totalTithe <= 0) return;

            totalTithe = Mathf.Min(totalTithe, seller.Money);
            seller.Money -= totalTithe;
            int captured = totalTithe;
            _inventoryRepository.UpdateCityInventoryState(cityId, s => s.GuildMoney += captured);
        }

        private bool HasGuildBuilding(string cityId)
        {
            if (_economyDatabase?.Cities == null) return false;
            foreach (var city in _economyDatabase.Cities)
            {
                if (city == null || city.Id != cityId) continue;
                if (city.Buildings == null) return false;
                foreach (var b in city.Buildings)
                    if (b == BuildingId.Guild) return true;
                return false;
            }
            return false;
        }

        private int PriceItems(string cityId, List<ItemStackState> items,
            out List<(ItemStackState stack, int price)> priced)
        {
            priced = new List<(ItemStackState, int)>();
            int total = 0;
            foreach (ItemStackState stack in items)
            {
                int price = _priceService.GetPrice(cityId, stack.ItemId, TradePriceKind.SellToCity,
                    applySkillBonus: false);
                priced.Add((stack, price));
                total += price * stack.Count;
            }
            return total;
        }

        private int SelectAffordableItems(
            List<(ItemStackState stack, int price)> priced, int cityMoney, int sellTotal,
            out List<(string itemId, int count)> soldItems)
        {
            soldItems = new List<(string, int)>();
            float ratio = cityMoney >= sellTotal ? 1f : (float)cityMoney / sellTotal;
            int totalReceived = 0;

            foreach ((ItemStackState stack, int price) in priced)
            {
                int countToSell = ratio >= 1f ? stack.Count : Mathf.FloorToInt(stack.Count * ratio);
                if (countToSell <= 0)
                    continue;

                totalReceived += price * countToSell;
                soldItems.Add((stack.ItemId, countToSell));
            }
            return totalReceived;
        }

        public int GetBuyPrice(string cityId, string itemId)
        {
            return _priceService.GetPrice(cityId, itemId, TradePriceKind.BuyFromCity,
                applySkillBonus: false);
        }

        public int GetSellPrice(string cityId, string itemId)
        {
            return _priceService.GetPrice(cityId, itemId, TradePriceKind.SellToCity,
                applySkillBonus: false);
        }

        public void ApplyBatchTrade(string cityId,
            Dictionary<string, int> toBuy, Dictionary<string, int> toSell, int npcMoney)
        {
            _inventoryRepository.UpdateCityInventory(cityId, cityInv =>
            {
                foreach (KeyValuePair<string, int> kvp in toBuy)
                    InventoryStateMutator.RemoveItems(cityInv, kvp.Key, kvp.Value);

                foreach (KeyValuePair<string, int> kvp in toSell)
                    InventoryStateMutator.AddItems(cityInv, kvp.Key, kvp.Value);

                cityInv.Money = Mathf.Max(0, npcMoney);
            });
        }
    }
}

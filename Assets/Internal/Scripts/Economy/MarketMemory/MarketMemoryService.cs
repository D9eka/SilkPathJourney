using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Events;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Save;
using Internal.Scripts.Trading;
using Zenject;

namespace Internal.Scripts.Economy.MarketMemory
{
    public sealed class MarketMemoryService : IInitializable, IDisposable
    {
        public const int RumorTtlDays = 14;

        private readonly SaveRepository _saveRepository;
        private readonly DayTracker _dayTracker;
        private readonly TradeModel _tradeModel;
        private readonly CityTradePriceService _priceService;
        private readonly InventoryRepository _inventoryRepository;
        private readonly ItemCatalog _itemCatalog;
        private readonly EconomyDatabase _economyDb;

        public MarketMemoryService(
            SaveRepository saveRepository,
            DayTracker dayTracker,
            TradeModel tradeModel,
            CityTradePriceService priceService,
            InventoryRepository inventoryRepository,
            ItemCatalog itemCatalog,
            EconomyDatabase economyDb)
        {
            _saveRepository = saveRepository;
            _dayTracker = dayTracker;
            _tradeModel = tradeModel;
            _priceService = priceService;
            _inventoryRepository = inventoryRepository;
            _itemCatalog = itemCatalog;
            _economyDb = economyDb;
        }

        public int CurrentDay => _dayTracker.CurrentDay;

        public void Initialize()
        {
            _tradeModel.OnCitySessionClosed += OnCitySessionClosed;
        }

        public void Dispose()
        {
            _tradeModel.OnCitySessionClosed -= OnCitySessionClosed;
        }

        public void RecordVisit(string cityId, IEnumerable<ItemPriceRecord> prices, int day, bool isRumor = false)
        {
            if (string.IsNullOrEmpty(cityId))
                return;

            var snapshot = new MarketSnapshot
            {
                CityId = cityId,
                Day = day,
                Prices = new List<ItemPriceRecord>(prices),
                IsRumor = isRumor
            };

            _saveRepository.Data.MarketMemory[cityId] = snapshot;
            _saveRepository.Save();
        }

        public MarketSnapshot Get(string cityId)
        {
            if (string.IsNullOrEmpty(cityId))
                return null;

            _saveRepository.Data.MarketMemory.TryGetValue(cityId, out MarketSnapshot snapshot);
            return snapshot;
        }

        public bool IsRumorExpired(MarketSnapshot snapshot, int currentDay)
        {
            if (snapshot == null || !snapshot.IsRumor)
                return false;
            return (currentDay - snapshot.Day) > RumorTtlDays;
        }

        public void RecordPriceTip(string cityId)
        {
            if (string.IsNullOrEmpty(cityId) || _economyDb == null)
                return;

            var records = new List<ItemPriceRecord>(_economyDb.Items.Count);
            foreach (ItemData item in _economyDb.Items)
            {
                if (item == null || string.IsNullOrEmpty(item.Id))
                    continue;

                int buy = _priceService.GetPrice(cityId, item.Id, TradePriceKind.BuyFromCity, false);
                int sell = _priceService.GetPrice(cityId, item.Id, TradePriceKind.SellToCity, false);
                records.Add(new ItemPriceRecord
                {
                    ItemId = item.Id,
                    BuyPrice = buy,
                    SellPrice = sell,
                    IsAvailable = buy > 0 || sell > 0
                });
            }

            RecordVisit(cityId, records, _dayTracker.CurrentDay, isRumor: true);
        }

        private void OnCitySessionClosed(string cityId)
        {
            var cityInv = _inventoryRepository.GetCityInventory(cityId);
            if (cityInv?.Inventory?.Items == null)
                return;

            var records = new List<ItemPriceRecord>(cityInv.Inventory.Items.Count);
            foreach (var stack in cityInv.Inventory.Items)
            {
                if (string.IsNullOrEmpty(stack.ItemId))
                    continue;

                int buy = _priceService.GetPrice(cityId, stack.ItemId, TradePriceKind.BuyFromCity, false);
                int sell = _priceService.GetPrice(cityId, stack.ItemId, TradePriceKind.SellToCity, false);
                records.Add(new ItemPriceRecord
                {
                    ItemId = stack.ItemId,
                    BuyPrice = buy,
                    SellPrice = sell,
                    IsAvailable = stack.Count > 0
                });
            }

            RecordVisit(cityId, records, _dayTracker.CurrentDay);
        }
    }
}

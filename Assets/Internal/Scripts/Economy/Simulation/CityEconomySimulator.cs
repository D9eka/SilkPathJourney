using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Events;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Economy.Simulation
{
    public sealed class CityEconomySimulator : IInitializable, IDisposable
    {
        private readonly DayTracker _dayTracker;
        private readonly InventoryRepository _inventoryRepository;
        private readonly CityMarketProfileService _profileService;
        private readonly EconomyDatabase _economyDatabase;
        private readonly ItemCatalog _itemCatalog;
        private readonly GuildSettings _guildSettings;

        private Dictionary<ItemType, List<ItemData>> _itemsByCategory;
        private Dictionary<string, (CityData city, CityTypeData type)> _cityLookup;

        public CityEconomySimulator(
            DayTracker dayTracker,
            InventoryRepository inventoryRepository,
            CityMarketProfileService profileService,
            EconomyDatabase economyDatabase,
            ItemCatalog itemCatalog,
            GuildSettings guildSettings)
        {
            _dayTracker = dayTracker;
            _inventoryRepository = inventoryRepository;
            _profileService = profileService;
            _economyDatabase = economyDatabase;
            _itemCatalog = itemCatalog;
            _guildSettings = guildSettings;
        }

        public void Initialize()
        {
            _dayTracker.OnDayChanged += OnDayChanged;
        }

        public void Dispose()
        {
            _dayTracker.OnDayChanged -= OnDayChanged;
        }

        public void SimulateDay()
        {
            BuildItemsLookupIfNeeded();

            bool isProductionDay = _dayTracker.CurrentDay % _guildSettings.ProductionIntervalDays == 0;

            _inventoryRepository.UpdateAllCityInventories(cityInventories =>
            {
                foreach (CityInventoryState cityState in cityInventories)
                {
                    if (cityState?.Inventory == null)
                        continue;

                    SimulateCity(cityState.CityId, cityState.Inventory);

                    if (!_cityLookup.TryGetValue(cityState.CityId, out var entry))
                        continue;
                    int income = Mathf.RoundToInt(entry.type.CityMoneyIncomePerScale * entry.city.MarketScale);
                    cityState.Inventory.Money += income;

                    bool hasGuild = entry.city.HasBuilding(BuildingId.Guild);
                    if (hasGuild && cityState.GuildMoney < _guildSettings.GuildRefillThreshold && cityState.Inventory.Money >= _guildSettings.GuildRefillAmount)
                    {
                        cityState.GuildMoney += _guildSettings.GuildRefillAmount;
                        cityState.Inventory.Money -= _guildSettings.GuildRefillAmount;
                    }

                    if (hasGuild)
                    {
                        int tax = Mathf.RoundToInt(income * _guildSettings.GuildCityTaxShare);
                        if (tax > 0 && cityState.Inventory.Money >= tax)
                        {
                            cityState.Inventory.Money -= tax;
                            cityState.GuildMoney += tax;
                        }
                    }

                    if (hasGuild && isProductionDay && entry.city.HasBuilding(BuildingId.Workshop))
                    {
                        foreach (GuildProductionEntry production in _guildSettings.ProductionProfile)
                        {
                            if (production.CityId != cityState.CityId)
                                continue;

                            int basePrice = _itemCatalog.GetItemPrice(production.ItemId);
                            int cost = Mathf.RoundToInt(production.CountPerTick * basePrice * _guildSettings.ProductionCostFactor);

                            if (cityState.GuildMoney >= cost)
                            {
                                cityState.GuildMoney -= cost;
                                InventoryStateMutator.AddItems(cityState.GuildInventory, production.ItemId, production.CountPerTick);
                            }
                        }
                    }
                }
            });
        }

        private void OnDayChanged(int day) => SimulateDay();

        private void SimulateCity(string cityId, InventoryState inventory)
        {
            foreach (KeyValuePair<ItemType, List<ItemData>> categoryGroup in _itemsByCategory)
            {
                foreach (ItemData item in categoryGroup.Value)
                {
                    if (!_profileService.TryGetProfile(cityId, item.Id, out CityItemMarketProfile profile))
                        continue;

                    if (!profile.HasDynamicMarket)
                        continue;

                    ItemStackState stack = inventory.Items.Find(s => s.ItemId == item.Id);
                    int current = stack?.Count ?? 0;

                    float next = current + profile.DailyNet
                                 + (profile.TargetStock - current) * profile.EquilibriumPull;
                    int nextInt = Mathf.Max(0, Mathf.RoundToInt(next));

                    if (nextInt > 0 && stack == null)
                    {
                        inventory.Items.Add(new ItemStackState
                        {
                            ItemId = item.Id,
                            Count = nextInt
                        });
                    }
                    else if (nextInt <= 0 && stack != null)
                    {
                        inventory.Items.Remove(stack);
                    }
                    else if (stack != null)
                    {
                        stack.Count = nextInt;
                    }
                }
            }
        }

        private void BuildItemsLookupIfNeeded()
        {
            if (_itemsByCategory != null)
                return;

            _itemsByCategory = new Dictionary<ItemType, List<ItemData>>();

            if (_economyDatabase.Items != null)
            {
                foreach (ItemData item in _economyDatabase.Items)
                {
                    if (item == null || item.Type == ItemType.Unknown || item.Type == ItemType.Quest)
                        continue;

                    if (!_itemsByCategory.TryGetValue(item.Type, out List<ItemData> list))
                    {
                        list = new List<ItemData>();
                        _itemsByCategory[item.Type] = list;
                    }

                    list.Add(item);
                }
            }

            _cityLookup = new Dictionary<string, (CityData, CityTypeData)>();
            if (_economyDatabase.Cities != null)
            {
                foreach (CityData city in _economyDatabase.Cities)
                {
                    if (city == null) continue;
                    CityTypeData cityType = _economyDatabase.GetCityType(city.Type);
                    if (cityType != null)
                        _cityLookup[city.Id] = (city, cityType);
                }
            }
        }
    }
}

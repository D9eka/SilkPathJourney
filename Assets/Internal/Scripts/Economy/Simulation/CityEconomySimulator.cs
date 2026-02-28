using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
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

        private Dictionary<ItemType, List<ItemData>> _itemsByCategory;

        public CityEconomySimulator(
            DayTracker dayTracker,
            InventoryRepository inventoryRepository,
            CityMarketProfileService profileService,
            EconomyDatabase economyDatabase,
            ItemCatalog itemCatalog)
        {
            _dayTracker = dayTracker;
            _inventoryRepository = inventoryRepository;
            _profileService = profileService;
            _economyDatabase = economyDatabase;
            _itemCatalog = itemCatalog;
        }

        public void Initialize()
        {
            _dayTracker.OnDayChanged += OnDayChanged;
        }

        public void Dispose()
        {
            _dayTracker.OnDayChanged -= OnDayChanged;
        }

        private void OnDayChanged(int day)
        {
            BuildItemsLookupIfNeeded();

            _inventoryRepository.UpdateAllCityInventories(cityInventories =>
            {
                foreach (CityInventoryState cityState in cityInventories)
                {
                    if (cityState?.Inventory == null)
                        continue;

                    SimulateCity(cityState.CityId, cityState.Inventory);
                }
            });
        }

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

            if (_economyDatabase.Items == null)
                return;

            foreach (ItemData item in _economyDatabase.Items)
            {
                if (item == null || item.Type == ItemType.Unknown)
                    continue;

                if (!_itemsByCategory.TryGetValue(item.Type, out List<ItemData> list))
                {
                    list = new List<ItemData>();
                    _itemsByCategory[item.Type] = list;
                }

                list.Add(item);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Player;
using UnityEngine;

namespace Internal.Scripts.Economy.Save
{
    public sealed class EconomySaveBuilder
    {
        private readonly EconomyDatabase _economyDatabase;
        private readonly PlayerConfig _playerConfig;
        private readonly EconomySimulationSettings _simulationSettings;

        private Dictionary<CityType, CityTypeData> _cityTypeByEnum;
        private Dictionary<ItemType, List<ItemData>> _itemsByCategory;
        private Dictionary<CultureCategoryKey, float> _cultureCategoryMult;
        private Dictionary<CultureItemKey, float> _cultureItemMult;

        public EconomySaveBuilder(
            EconomyDatabase economyDatabase,
            PlayerConfig playerConfig,
            EconomySimulationSettings simulationSettings)
        {
            _economyDatabase = economyDatabase;
            _playerConfig = playerConfig;
            _simulationSettings = simulationSettings;
        }

        public EconomySaveData Build()
        {
            EconomySaveData data = new EconomySaveData();
            data.PlayerInventory = CreatePlayerInventory();

            if (_economyDatabase.Cities != null)
            {
                foreach (CityData city in _economyDatabase.Cities)
                {
                    if (city == null || string.IsNullOrWhiteSpace(city.Id))
                        continue;

                    data.CityInventories.Add(new CityInventoryState
                    {
                        CityId = city.Id,
                        Inventory = CreateCityInventory(city)
                    });
                }
            }

            data.IsInitialized = true;
            return data;
        }

        private InventoryState CreatePlayerInventory()
        {
            InventoryState inv = new InventoryState
            {
                Money = _playerConfig.StartMoney,
                MaxWeightKg = _playerConfig.MaxWeightKg,
                Items = new List<ItemStackState>()
            };

            if (_playerConfig.StartItems != null)
            {
                foreach (PlayerConfig.StartItemEntry entry in _playerConfig.StartItems)
                {
                    if (string.IsNullOrWhiteSpace(entry.ItemId) || entry.Count <= 0)
                        continue;

                    inv.Items.Add(new ItemStackState
                    {
                        ItemId = entry.ItemId,
                        Count = entry.Count
                    });
                }
            }

            return inv;
        }

        private InventoryState CreateCityInventory(CityData city)
        {
            BuildLookupsIfNeeded();

            InventoryState inv = new InventoryState
            {
                Money = CalculateCityMoney(city),
                MaxWeightKg = -1f,
                Items = new List<ItemStackState>()
            };

            if (!_cityTypeByEnum.TryGetValue(city.Type, out CityTypeData typeData) || typeData == null)
                return inv;

            foreach (CityTypeData.CategoryStockProfile profile in typeData.CategoryStockProfiles)
            {
                if (profile.Category == ItemType.Unknown)
                    continue;

                float desiredCategory = profile.DesiredPerScale * city.MarketScale;
                if (!_itemsByCategory.TryGetValue(profile.Category, out List<ItemData> itemsInCategory) || itemsInCategory.Count == 0)
                    continue;

                float sumW = 0f;
                Dictionary<ItemData, float> weights = new Dictionary<ItemData, float>();
                foreach (ItemData item in itemsInCategory)
                {
                    float w = CalculateEffectiveWeight(city, profile.Category, item);
                    if (w <= 0f)
                        continue;

                    weights[item] = w;
                    sumW += w;
                }

                if (sumW <= 0f)
                    continue;

                foreach (KeyValuePair<ItemData, float> kvp in weights)
                {
                    ItemData item = kvp.Key;
                    float w = kvp.Value;

                    int desiredItem = Mathf.RoundToInt(desiredCategory * w / sumW);
                    if (desiredItem < 1)
                        desiredItem = 1;

                    int stock = ApplyInitialStockVariance(city.Id, item.Id, desiredItem);
                    if (stock <= 0)
                        continue;

                    inv.Items.Add(new ItemStackState
                    {
                        ItemId = item.Id,
                        Count = stock
                    });
                }
            }

            return inv;
        }

        private int CalculateCityMoney(CityData city)
        {
            BuildLookupsIfNeeded();

            if (_cityTypeByEnum.TryGetValue(city.Type, out CityTypeData typeData) && typeData != null)
            {
                float money = typeData.CityMoneyIncomePerScale * city.MarketScale;
                return Mathf.Max(0, Mathf.RoundToInt(money));
            }

            return 0;
        }

        private float CalculateEffectiveWeight(CityData city, ItemType category, ItemData item)
        {
            float weight = Mathf.Max(0f, item.DemandWeight);

            CultureId primary = city.PrimaryCulture;
            CultureId secondary = city.SecondaryCulture;

            weight *= GetCultureCategoryMultiplier(primary, category);
            weight *= GetCultureItemMultiplier(primary, item.Id);

            if (secondary != CultureId.None)
            {
                float secondaryCat = GetCultureCategoryMultiplier(secondary, category);
                float secondaryItem = GetCultureItemMultiplier(secondary, item.Id);
                weight *= Mathf.Sqrt(secondaryCat * secondaryItem);
            }

            return weight;
        }

        private float GetCultureCategoryMultiplier(CultureId culture, ItemType category)
        {
            if (culture == CultureId.None || category == ItemType.Unknown)
                return 1f;

            return _cultureCategoryMult.GetValueOrDefault(new CultureCategoryKey(culture, category), 1f);
        }

        private float GetCultureItemMultiplier(CultureId culture, string itemId)
        {
            if (culture == CultureId.None || string.IsNullOrWhiteSpace(itemId))
                return 1f;

            return _cultureItemMult.GetValueOrDefault(new CultureItemKey(culture, itemId), 1f);
        }

        private int ApplyInitialStockVariance(string cityId, string itemId, int desired)
        {
            float ratio = _simulationSettings.InitialStockRatio;
            float variation = _simulationSettings.InitialStockVariationPct;

            float noise = Hash01($"{cityId}|{itemId}");
            float factor = 1f + (noise * 2f - 1f) * variation;
            float value = desired * ratio * factor;

            int stock = Mathf.RoundToInt(value);
            return Mathf.Max(0, stock);
        }

        private void BuildLookupsIfNeeded()
        {
            if (_cityTypeByEnum != null)
                return;

            _cityTypeByEnum = new Dictionary<CityType, CityTypeData>();
            _itemsByCategory = new Dictionary<ItemType, List<ItemData>>();
            _cultureCategoryMult = new Dictionary<CultureCategoryKey, float>();
            _cultureItemMult = new Dictionary<CultureItemKey, float>();

            if (_economyDatabase.CityTypes != null)
            {
                foreach (CityTypeData ct in _economyDatabase.CityTypes)
                {
                    if (ct == null)
                        continue;
                    _cityTypeByEnum[ct.Type] = ct;
                }
            }

            if (_economyDatabase.Items != null)
            {
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

            if (_economyDatabase.CultureCategoryDemandMultipliers != null)
            {
                foreach (EconomyDatabase.CultureCategoryDemandMultiplier entry in _economyDatabase.CultureCategoryDemandMultipliers)
                {
                    CultureCategoryKey key = new CultureCategoryKey(entry.Culture, entry.Category);
                    if (!_cultureCategoryMult.ContainsKey(key))
                        _cultureCategoryMult[key] = entry.Multiplier;
                }
            }

            if (_economyDatabase.CultureItemDemandMultipliers != null)
            {
                foreach (EconomyDatabase.CultureItemDemandMultiplier entry in _economyDatabase.CultureItemDemandMultipliers)
                {
                    if (string.IsNullOrWhiteSpace(entry.ItemId))
                        continue;
                    CultureItemKey key = new CultureItemKey(entry.Culture, entry.ItemId);
                    if (!_cultureItemMult.ContainsKey(key))
                        _cultureItemMult[key] = entry.Multiplier;
                }
            }
        }

        private static float Hash01(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0f;

            unchecked
            {
                int hash = 23;
                for (int i = 0; i < value.Length; i++)
                    hash = hash * 31 + value[i];

                int positive = hash & int.MaxValue;
                return positive / (float)int.MaxValue;
            }
        }

        private readonly struct CultureCategoryKey : IEquatable<CultureCategoryKey>
        {
            public CultureCategoryKey(CultureId culture, ItemType category)
            {
                Culture = culture;
                Category = category;
            }

            public CultureId Culture { get; }
            public ItemType Category { get; }

            public bool Equals(CultureCategoryKey other) => Culture == other.Culture && Category == other.Category;
            public override bool Equals(object obj) => obj is CultureCategoryKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine((int)Culture, (int)Category);
        }

        private readonly struct CultureItemKey : IEquatable<CultureItemKey>
        {
            public CultureItemKey(CultureId culture, string itemId)
            {
                Culture = culture;
                ItemId = itemId ?? string.Empty;
            }

            public CultureId Culture { get; }
            public string ItemId { get; }

            public bool Equals(CultureItemKey other) => Culture == other.Culture && ItemId == other.ItemId;
            public override bool Equals(object obj) => obj is CultureItemKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine((int)Culture, ItemId);
        }
    }
}
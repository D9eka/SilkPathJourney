using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Meta;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Background;
using UnityEngine;

namespace Internal.Scripts.Economy.Save
{
    public sealed class EconomySaveBuilder
    {
        private readonly EconomyDatabase _economyDatabase;
        private readonly PlayerConfig _playerConfig;
        private readonly CaravanDatabase _caravanDatabase;
        private readonly EconomySimulationSettings _simulationSettings;
        private readonly GuildSettings _guildSettings;
        private readonly PersistentProgressService _persistent;

        private Dictionary<CityType, CityTypeData> _cityTypeByEnum;
        private Dictionary<ItemType, List<ItemData>> _itemsByCategory;
        private Dictionary<CultureCategoryKey, float> _cultureCategoryMult;
        private Dictionary<CultureItemKey, float> _cultureItemMult;

        public EconomySaveBuilder(
            EconomyDatabase economyDatabase,
            PlayerConfig playerConfig,
            CaravanDatabase caravanDatabase,
            EconomySimulationSettings simulationSettings,
            GuildSettings guildSettings,
            PersistentProgressService persistent)
        {
            _economyDatabase = economyDatabase;
            _playerConfig = playerConfig;
            _caravanDatabase = caravanDatabase;
            _simulationSettings = simulationSettings;
            _guildSettings = guildSettings;
            _persistent = persistent;
        }

        public EconomySaveData Build(BackgroundData background = null, CartClassData cartClass = null)
        {
            EconomySaveData data = new EconomySaveData();
            data.PlayerInventory = CreatePlayerInventory(background);
            data.PlayerResources = CreatePlayerResources(background, cartClass);

            if (_economyDatabase.Cities != null)
            {
                foreach (CityData city in _economyDatabase.Cities)
                {
                    if (city == null || string.IsNullOrWhiteSpace(city.Id))
                        continue;

                    data.CityInventories.Add(new CityInventoryState
                    {
                        CityId = city.Id,
                        Inventory = CreateCityInventory(city),
                        GuildMoney = city.HasBuilding(BuildingId.Guild) ? _guildSettings.GuildStartingMoney : 0,
                        GuildInventory = CreateGuildInventory(city)
                    });
                }
            }

            data.IsInitialized = true;
            return data;
        }

        private InventoryState CreatePlayerInventory(BackgroundData background)
        {
            InventoryState inv = new InventoryState { Items = new List<ItemStackState>() };

            int startSupplies = background != null ? background.StartingSupplies : Mathf.RoundToInt(_playerConfig.StartFood);
            InventoryStateMutator.AddItems(inv, SuppliesItemId.Value, startSupplies);

            if (background != null)
            {
                foreach (ItemStartEntry entry in background.StartingItems)
                    InventoryStateMutator.AddItems(inv, entry.ItemId, entry.Count);
            }
            else if (_playerConfig.StartItems != null)
            {
                foreach (PlayerConfig.StartItemEntry entry in _playerConfig.StartItems)
                    InventoryStateMutator.AddItems(inv, entry.ItemId, entry.Count);
            }

            const string StartItemPrefix = "unlock_startitem_";
            foreach (string id in _persistent.UnlockedIds)
            {
                if (!id.StartsWith(StartItemPrefix, System.StringComparison.Ordinal)) continue;
                string itemId = id.Substring(StartItemPrefix.Length);
                InventoryStateMutator.AddItems(inv, itemId, 1);
            }

            return inv;
        }

        private PlayerResourceState CreatePlayerResources(BackgroundData background, CartClassData cartClass)
        {
            CartClassData classData = cartClass ?? _caravanDatabase.GetCartClass(_playerConfig.StartCartClass);
            string classId = classData != null ? classData.Id : PlayerResourceState.DEFAULT_CART_CLASS;
            int money = background != null ? background.StartingMoney : _playerConfig.StartMoney;
            int reputation = background != null ? background.StartingReputation : 25;
            float morale = background != null
                ? PlayerResourceState.MORALE_MAX / 2f + background.StartingMoraleBonus
                : PlayerResourceState.MORALE_MAX / 2f;

            PlayerResourceState resources = new PlayerResourceState
            {
                Money = money,
                Food = 0f,
                AccumulatedDanger = 0f,
                PlayerCart = CreatePlayerCart(classData),
                Carts = new List<CartState>(),
                CartClassId = classId,
                CartUpgradeLevelId = PlayerResourceState.DEFAULT_UPGRADE_LEVEL,
                DraftAnimalId = PlayerResourceState.DEFAULT_DRAFT_ANIMAL,
                Companions = new List<CompanionState>(),
                ActiveUpgrades = BuildLegacyActiveUpgrades(),
                Reputation = reputation,
                Morale = Mathf.Clamp(morale, PlayerResourceState.MORALE_MIN, PlayerResourceState.MORALE_MAX)
            };

            if (_playerConfig.StartCarts != null)
            {
                foreach (PlayerConfig.StartCartEntry entry in _playerConfig.StartCarts)
                {
                    resources.Carts.Add(new CartState
                    {
                        Capacity = entry.Capacity,
                        Durability = entry.Durability,
                        MaxDurability = entry.Durability
                    });
                }
            }

            return resources;
        }

        private List<string> BuildLegacyActiveUpgrades()
        {
            const string UpgradePrefix = "unlock_upgrade_";
            var result = new List<string>();
            foreach (string id in _persistent.UnlockedIds)
            {
                if (!id.StartsWith(UpgradePrefix, System.StringComparison.Ordinal)) continue;
                result.Add(id.Substring(UpgradePrefix.Length).Replace("_", ""));
            }
            return result;
        }

        private CartState CreatePlayerCart(CartClassData classData)
        {
            if (classData == null)
            {
                return new CartState
                {
                    Capacity = 250f,
                    Durability = 100f,
                    MaxDurability = 100f,
                    Speed = 30f
                };
            }

            return new CartState
            {
                Capacity = classData.Capacity,
                Durability = classData.Durability,
                MaxDurability = classData.Durability,
                Speed = classData.SpeedKmDay
            };
        }

        private InventoryState CreateCityInventory(CityData city)
        {
            BuildLookupsIfNeeded();

            InventoryState inv = new InventoryState
            {
                Money = CalculateCityMoney(city),
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

        private InventoryState CreateGuildInventory(CityData city)
        {
            var inv = new InventoryState { Items = new List<ItemStackState>() };

            if (!city.HasBuilding(BuildingId.Guild))
                return inv;

            BuildLookupsIfNeeded();

            if (!_cityTypeByEnum.TryGetValue(city.Type, out CityTypeData typeData) || typeData == null)
                return inv;

            foreach (CityTypeData.CategoryStockProfile profile in typeData.CategoryStockProfiles)
            {
                if (profile.Category != ItemType.Craft
                    && profile.Category != ItemType.Luxury
                    && profile.Category != ItemType.Exotic)
                    continue;

                if (!_itemsByCategory.TryGetValue(profile.Category, out List<ItemData> items) || items.Count == 0)
                    continue;

                var ranked = new List<(ItemData item, float weight)>();
                foreach (ItemData item in items)
                {
                    float w = CalculateEffectiveWeight(city, profile.Category, item);
                    if (w > 0f)
                        ranked.Add((item, w));
                }
                ranked.Sort((a, b) => b.weight.CompareTo(a.weight));

                int take = Mathf.Min(2, ranked.Count);
                for (int i = 0; i < take; i++)
                {
                    float noise = Hash01($"guild_{city.Id}_{ranked[i].item.Id}");
                    int count = Mathf.RoundToInt(3f + noise * 5f);
                    inv.Items.Add(new ItemStackState
                    {
                        ItemId = ranked[i].item.Id,
                        Count = count
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
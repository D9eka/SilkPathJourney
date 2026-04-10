using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Trading;
using UnityEngine;

namespace Internal.Scripts.Economy.Simulation
{
    public sealed class CityTradePriceService
    {
        private readonly CityMarketProfileService _profileService;
        private readonly EconomySimulationSettings _settings;
        private readonly InventoryRepository _inventoryRepository;
        private readonly ItemCatalog _itemCatalog;
        private readonly TradePriceModifiers _modifiers;
        private readonly EconomyDatabase _economyDatabase;
        private readonly CultureDistanceService _cultureDistance;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly GuildSettings _guildSettings;
        private readonly HashSet<string> _warnedKeys = new();

        public CityTradePriceService(
            CityMarketProfileService profileService,
            EconomySimulationSettings settings,
            InventoryRepository inventoryRepository,
            ItemCatalog itemCatalog,
            TradePriceModifiers modifiers,
            EconomyDatabase economyDatabase,
            CultureDistanceService cultureDistance,
            GameBalanceConfig balanceConfig,
            GuildSettings guildSettings)
        {
            _profileService = profileService;
            _settings = settings;
            _inventoryRepository = inventoryRepository;
            _itemCatalog = itemCatalog;
            _modifiers = modifiers;
            _economyDatabase = economyDatabase;
            _cultureDistance = cultureDistance;
            _balanceConfig = balanceConfig;
            _guildSettings = guildSettings;
        }

        public int GetPrice(string cityId, string itemId, TradePriceKind kind,
            bool applySkillBonus = true)
            => CalculatePrice(cityId, itemId, kind, applySkillBonus).FinalPrice;

        public int GetPriceWithExoticity(string cityId, string itemId, TradePriceKind kind,
            CultureId originCulture, bool applySkillBonus = true)
            => GetPriceBreakdownWithExoticity(cityId, itemId, kind, originCulture, applySkillBonus).FinalPrice;

        public PriceBreakdown GetPriceBreakdown(string cityId, string itemId, TradePriceKind kind)
            => CalculatePrice(cityId, itemId, kind, true);

        public PriceBreakdown GetPriceBreakdownWithExoticity(
            string cityId, string itemId, TradePriceKind kind, CultureId originCulture,
            bool applySkillBonus = true)
        {
            PriceBreakdown breakdown = CalculatePrice(cityId, itemId, kind, applySkillBonus);
            if (kind != TradePriceKind.SellToCity || originCulture == CultureId.None)
                return breakdown;

            CityData city = _economyDatabase.Cities.Find(c => c.Id == cityId);
            if (city == null)
                return breakdown;

            float exoticityMult = _cultureDistance.GetExoticityMultiplier(
                originCulture, city.PrimaryCulture, _balanceConfig.ExoticityPerStep);
            int finalPrice = Mathf.Max(1, Mathf.RoundToInt(breakdown.FinalPrice * exoticityMult));
            return new PriceBreakdown(breakdown.ItemName, breakdown.BasePrice, breakdown.MarketMult,
                breakdown.BonusMult, breakdown.ModifierMult, exoticityMult, breakdown.TitheMult, finalPrice, breakdown.IsNpcTrade);
        }

        private PriceBreakdown CalculatePrice(string cityId, string itemId, TradePriceKind kind, bool applySkillBonus)
        {
            ItemData item = _itemCatalog.GetItem(itemId);
            string itemName = _itemCatalog.ResolveItemName(itemId);

            if (item == null)
                return new PriceBreakdown(itemName, 0, 1f, 1f, 1f, 1f, 0, false);

            if (!_profileService.TryGetProfile(cityId, itemId, out CityItemMarketProfile profile))
                return new PriceBreakdown(itemName, item.BasePrice, 1f, 1f, 1f, 1f, item.BasePrice, false);

            float baseCoef = kind == TradePriceKind.BuyFromCity ? profile.BuyCoef : profile.SellCoef;

            float scarcityMult = 1f;
            if (profile.HasDynamicMarket && profile.TargetStock > 0f)
            {
                int currentStock = GetCurrentStock(cityId, itemId);
                float stockPressure = (profile.TargetStock - currentStock) / Mathf.Max(1f, profile.TargetStock);
                scarcityMult = Mathf.Clamp(
                    1f + stockPressure * _settings.PriceScarcityStrength,
                    _settings.PriceMultiplierMin,
                    _settings.PriceMultiplierMax);
            }

            float bonusMult = 1f;
            float worldModMult = 1f;
            float titheMult = 1f;
            if (applySkillBonus)
            {
                bonusMult = kind == TradePriceKind.BuyFromCity
                    ? _modifiers.GetBuyBonusMultiplier(cityId)
                    : _modifiers.GetSellBonusMultiplier(cityId);
                worldModMult = _modifiers.GetWorldModifierMultiplier(cityId);

                if (kind == TradePriceKind.SellToCity && HasGuildBuilding(cityId))
                {
                    titheMult = item.Type switch
                    {
                        ItemType.Craft => 1f - _guildSettings.TitheCraftPct,
                        ItemType.Luxury => 1f - _guildSettings.TitheLuxuryPct,
                        ItemType.Exotic => 1f - _guildSettings.TitheExoticPct,
                        _ => 1f
                    };
                }
            }

            float marketMult = baseCoef * scarcityMult;
            int finalPrice = Mathf.Max(1, Mathf.RoundToInt(item.BasePrice * marketMult * bonusMult * worldModMult * titheMult));
            return new PriceBreakdown(itemName, item.BasePrice, marketMult, bonusMult, worldModMult, titheMult, finalPrice, false);
        }

        private bool HasGuildBuilding(string cityId)
            => _economyDatabase?.Cities?.Find(c => c.Id == cityId)?.HasBuilding(BuildingId.Guild) == true;

        private int GetCurrentStock(string cityId, string itemId)
        {
            CityInventoryState cityState = _inventoryRepository.GetCityInventory(cityId);
            if (cityState?.Inventory?.Items == null)
            {
                WarnOnce($"inv_{cityId}", $"[SPJ Economy] Missing city inventory for city='{cityId}'. Using zero stock for pricing.");
                return 0;
            }

            ItemStackState stack = cityState.Inventory.Items.Find(s => s.ItemId == itemId);
            return stack?.Count ?? 0;
        }

        private void WarnOnce(string key, string message)
        {
            if (_warnedKeys.Add(key))
                Debug.LogWarning(message);
        }
    }
}

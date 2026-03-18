using System.Collections.Generic;
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
        private readonly HashSet<string> _warnedKeys = new();

        public CityTradePriceService(
            CityMarketProfileService profileService,
            EconomySimulationSettings settings,
            InventoryRepository inventoryRepository,
            ItemCatalog itemCatalog,
            TradePriceModifiers modifiers)
        {
            _profileService = profileService;
            _settings = settings;
            _inventoryRepository = inventoryRepository;
            _itemCatalog = itemCatalog;
            _modifiers = modifiers;
        }

        public int GetPrice(string cityId, string itemId, TradePriceKind kind,
            bool applySkillBonus = true)
            => CalculatePrice(cityId, itemId, kind, applySkillBonus).FinalPrice;

        public PriceBreakdown GetPriceBreakdown(string cityId, string itemId, TradePriceKind kind)
            => CalculatePrice(cityId, itemId, kind, true);

        private PriceBreakdown CalculatePrice(string cityId, string itemId, TradePriceKind kind, bool applySkillBonus)
        {
            ItemData item = _itemCatalog.GetItem(itemId);
            string itemName = _itemCatalog.ResolveItemName(itemId);

            if (item == null)
                return new PriceBreakdown(itemName, 0, 1f, 1f, 1f, 0, false);

            if (!_profileService.TryGetProfile(cityId, itemId, out CityItemMarketProfile profile))
                return new PriceBreakdown(itemName, item.BasePrice, 1f, 1f, 1f, item.BasePrice, false);

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
            if (applySkillBonus)
            {
                bonusMult = kind == TradePriceKind.BuyFromCity
                    ? _modifiers.GetBuyBonusMultiplier(cityId)
                    : _modifiers.GetSellBonusMultiplier(cityId);
                worldModMult = _modifiers.GetWorldModifierMultiplier(cityId);
            }

            float marketMult = baseCoef * scarcityMult;
            int finalPrice = Mathf.Max(1, Mathf.RoundToInt(item.BasePrice * marketMult * bonusMult * worldModMult));
            return new PriceBreakdown(itemName, item.BasePrice, marketMult, bonusMult, worldModMult, finalPrice, false);
        }

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

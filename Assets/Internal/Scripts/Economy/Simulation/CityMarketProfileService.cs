using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Items;
using Internal.Scripts.Items;
using UnityEngine;

namespace Internal.Scripts.Economy.Simulation
{
    public sealed class CityMarketProfileService
    {
        private readonly EconomyDatabase _economyDatabase;
        private readonly ItemCatalog _itemCatalog;

        private Dictionary<string, CityData> _citiesById;
        private Dictionary<CityType, CityTypeData> _cityTypeByEnum;
        private Dictionary<(CityType, ItemType), CityTypeData.CategoryCoef> _coefLookup;
        private Dictionary<(CityType, ItemType), CityTypeData.CategoryStockProfile> _profileLookup;
        private readonly HashSet<string> _warnedKeys = new();

        public CityMarketProfileService(EconomyDatabase economyDatabase, ItemCatalog itemCatalog)
        {
            _economyDatabase = economyDatabase;
            _itemCatalog = itemCatalog;
        }

        public bool TryGetProfile(string cityId, string itemId, out CityItemMarketProfile profile)
        {
            BuildLookupsIfNeeded();

            profile = CityItemMarketProfile.Fallback;

            if (string.IsNullOrWhiteSpace(cityId) || string.IsNullOrWhiteSpace(itemId))
                return false;

            ItemData item = _itemCatalog.GetItem(itemId);
            if (item == null)
            {
                WarnOnce($"item_{itemId}", $"[SPJ Economy] Unknown item '{itemId}' requested by city market.");
                return false;
            }

            if (item.Type == ItemType.Unknown)
                return false;

            if (!_citiesById.TryGetValue(cityId, out CityData city) || city == null)
            {
                WarnOnce($"city_{cityId}", $"[SPJ Economy] Unknown city '{cityId}' requested by market profile.");
                return false;
            }

            CityType cityType = city.Type;
            ItemType category = item.Type;

            bool hasCoef = _coefLookup.TryGetValue((cityType, category), out CityTypeData.CategoryCoef coef);
            bool hasStockProfile = _profileLookup.TryGetValue((cityType, category), out CityTypeData.CategoryStockProfile stockProfile);

            if (!hasCoef && !hasStockProfile)
            {
                WarnOnce($"profile_{cityType}_{category}",
                    $"[SPJ Economy] Missing market profile for city='{cityId}', item='{itemId}', category='{category}'. Fallback pricing will be used.");
                return false;
            }

            float buyCoef = hasCoef ? coef.BuyCoef : 1f;
            float sellCoef = hasCoef ? coef.SellCoef : 1f;
            float targetStock = hasStockProfile ? stockProfile.DesiredPerScale * city.MarketScale : 0f;
            float dailyNet = hasStockProfile ? stockProfile.DailyNet : 0f;
            float equilibriumPull = hasStockProfile ? stockProfile.EquilibriumPull : 0f;
            bool hasDynamic = hasCoef && hasStockProfile && stockProfile.DesiredPerScale > 0f;

            if (hasCoef && !hasStockProfile)
            {
                WarnOnce($"stockprofile_{cityType}_{category}",
                    $"[SPJ Economy] Missing stock profile: city_type='{cityType}', category='{category}'. Dynamic market disabled for this pair.");
            }
            else if (!hasCoef && hasStockProfile)
            {
                WarnOnce($"coef_{cityType}_{category}",
                    $"[SPJ Economy] Missing category coef: city_type='{cityType}', category='{category}'. Using default coefs (1.0/1.0).");
            }

            profile = new CityItemMarketProfile(targetStock, dailyNet, equilibriumPull, buyCoef, sellCoef, hasDynamic);
            return true;
        }

        private void WarnOnce(string key, string message)
        {
            if (_warnedKeys.Add(key))
                Debug.LogWarning(message);
        }

        private void BuildLookupsIfNeeded()
        {
            if (_citiesById != null)
                return;

            _citiesById = new Dictionary<string, CityData>();
            _cityTypeByEnum = new Dictionary<CityType, CityTypeData>();
            _coefLookup = new Dictionary<(CityType, ItemType), CityTypeData.CategoryCoef>();
            _profileLookup = new Dictionary<(CityType, ItemType), CityTypeData.CategoryStockProfile>();

            if (_economyDatabase.Cities != null)
            {
                foreach (CityData city in _economyDatabase.Cities)
                {
                    if (city != null && !string.IsNullOrWhiteSpace(city.Id))
                        _citiesById.TryAdd(city.Id, city);
                }
            }

            if (_economyDatabase.CityTypes != null)
            {
                foreach (CityTypeData ct in _economyDatabase.CityTypes)
                {
                    if (ct == null)
                        continue;

                    _cityTypeByEnum[ct.Type] = ct;

                    foreach (CityTypeData.CategoryCoef coef in ct.CategoryCoefs)
                    {
                        if (coef.Category != ItemType.Unknown)
                            _coefLookup.TryAdd((ct.Type, coef.Category), coef);
                    }

                    foreach (CityTypeData.CategoryStockProfile profile in ct.CategoryStockProfiles)
                    {
                        if (profile.Category != ItemType.Unknown)
                            _profileLookup.TryAdd((ct.Type, profile.Category), profile);
                    }
                }
            }
        }
    }
}

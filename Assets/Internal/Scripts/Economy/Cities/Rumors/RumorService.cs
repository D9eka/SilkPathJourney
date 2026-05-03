using System;
using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.MarketMemory;
using Internal.Scripts.Events;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.WorldModifiers;

namespace Internal.Scripts.Economy.Cities.Rumors
{
    public sealed class RumorService
    {
        private readonly CityRadiusService _radiusResolver;
        private readonly WorldModifierRepository _modifierRepository;
        private readonly DayTracker _dayTracker;
        private readonly GameBalanceConfig _config;
        private readonly EconomyDatabase _economyDb;
        private readonly MarketMemoryService _marketMemory;

        public RumorService(
            CityRadiusService radiusResolver,
            WorldModifierRepository modifierRepository,
            DayTracker dayTracker,
            GameBalanceConfig config,
            EconomyDatabase economyDb,
            MarketMemoryService marketMemory)
        {
            _radiusResolver = radiusResolver;
            _modifierRepository = modifierRepository;
            _dayTracker = dayTracker;
            _config = config;
            _economyDb = economyDb;
            _marketMemory = marketMemory;
        }

        public List<RumorData> GetAvailableRumors(string cityId)
        {
            var result = new List<RumorData>();
            int currentDay = _dayTracker.CurrentDay;

            string currentNodeId = _economyDb.Cities.Find(c =>
                string.Equals(c.Id, cityId, StringComparison.OrdinalIgnoreCase))?.NodeId ?? "";
            var cities = _radiusResolver.GetCitiesInRadius(currentNodeId, _config.RumorRadius);
            foreach (var city in cities)
            {
                var modifiers = _modifierRepository.GetCityModifiers(city.Id);
                foreach (var entry in modifiers)
                {
                    if (entry.LastSeenDay >= 0)
                        continue;

                    var modifierData = _economyDb.GetCityModifier(entry.ModifierId);
                    string modifierName = modifierData != null
                        ? LocalizationService.ResolveString(modifierData.Name, entry.ModifierId, "RumorService")
                        : entry.ModifierId;

                    int remainingDays = entry.StartDay + entry.Duration - currentDay;
                    result.Add(new RumorData(city, entry.ModifierId, modifierName, entry.StartDay, remainingDays));
                }
            }

            return result;
        }

        public List<PriceTipData> GetAvailablePriceTips(string currentCityId)
        {
            string currentNodeId = _economyDb.Cities.Find(c =>
                string.Equals(c.Id, currentCityId, StringComparison.OrdinalIgnoreCase))?.NodeId ?? "";

            int maxRadius = _config.RumorRadius + 2;
            var result = new List<PriceTipData>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            seen.Add(currentCityId);

            for (int hop = 1; hop <= maxRadius; hop++)
            {
                var citiesAtHop = _radiusResolver.GetCitiesInRadius(currentNodeId, hop);
                foreach (var city in citiesAtHop)
                {
                    if (!seen.Add(city.Id))
                        continue;

                    int cost = _config.GetPriceTipCost(hop);
                    result.Add(new PriceTipData(city, hop, cost));
                }
            }

            return result;
        }

        public void PurchaseRumors(string cityId)
        {
            int currentDay = _dayTracker.CurrentDay;
            _modifierRepository.MarkCitySeen(cityId, currentDay);
        }

        public void PurchasePriceTip(string targetCityId)
        {
            _marketMemory.RecordPriceTip(targetCityId);
        }

        public int GetRumorCost() => _config.RumorCost;
    }
}

using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Npc.Save;
using UnityEngine;

namespace Internal.Scripts.Npc.Trading
{
    public sealed class NpcSellEstimator
    {
        private readonly CityTradePriceService _priceService;
        private readonly ItemCatalog _itemCatalog;
        private readonly EconomyDatabase _economyDatabase;
        private readonly CultureDistanceService _cultureDistance;
        private readonly GameBalanceConfig _balanceConfig;

        public NpcSellEstimator(
            CityTradePriceService priceService,
            ItemCatalog itemCatalog,
            EconomyDatabase economyDatabase,
            CultureDistanceService cultureDistance,
            GameBalanceConfig balanceConfig)
        {
            _priceService = priceService;
            _itemCatalog = itemCatalog;
            _economyDatabase = economyDatabase;
            _cultureDistance = cultureDistance;
            _balanceConfig = balanceConfig;
        }

        public int EstimateSellPrice(NpcAgentSaveState agent, string targetCityId, string itemId) =>
            Estimate(agent.Experience, targetCityId, itemId, CultureId.None);

        public int EstimateSellPrice(NpcEconomyState agent, string targetCityId, string itemId) =>
            Estimate(agent.Experience, targetCityId, itemId, CultureId.None);

        public int EstimateSellPrice(NpcEconomyState agent, string targetCityId, string itemId, CultureId originCulture) =>
            Estimate(agent.Experience, targetCityId, itemId, originCulture);

        private int Estimate(NpcExperienceLevel experience, string targetCityId, string itemId, CultureId originCulture)
        {
            var item = _itemCatalog.GetItem(itemId);
            if (item == null)
                return 1;

            int baseEstimate;
            switch (experience)
            {
                case NpcExperienceLevel.Novice:
                    baseEstimate = EstimateNovice(item, targetCityId);
                    break;

                case NpcExperienceLevel.Experienced:
                {
                    int real = _priceService.GetPrice(targetCityId, itemId, TradePriceKind.SellToCity,
                        applySkillBonus: false);
                    float noise = Random.Range(0.8f, 1.2f);
                    baseEstimate = Mathf.Max(1, Mathf.RoundToInt(real * noise));
                    break;
                }

                default:
                {
                    int real = _priceService.GetPrice(targetCityId, itemId, TradePriceKind.SellToCity,
                        applySkillBonus: false);
                    float noise = Random.Range(0.9f, 1.1f);
                    baseEstimate = Mathf.Max(1, Mathf.RoundToInt(real * noise));
                    break;
                }
            }

            if (originCulture != CultureId.None)
            {
                CityData targetCity = _economyDatabase.Cities.Find(c => c.Id == targetCityId);
                if (targetCity != null)
                {
                    float exoticityMult = _cultureDistance.GetExoticityMultiplier(
                        originCulture, targetCity.PrimaryCulture, _balanceConfig.ExoticityPerStep);
                    baseEstimate = Mathf.Max(1, Mathf.RoundToInt(baseEstimate * exoticityMult));
                }
            }

            return baseEstimate;
        }

        private int EstimateNovice(Economy.Items.ItemData item, string cityId)
        {
            var city = _economyDatabase.Cities.Find(c => c.Id == cityId);
            if (city == null)
                return Mathf.Max(1, item.BasePrice);

            CityTypeData cityType = _economyDatabase.GetCityType(city.Type);
            if (cityType == null)
                return Mathf.Max(1, item.BasePrice);

            float sellCoef = 1f;
            foreach (CityTypeData.CategoryCoef coef in cityType.CategoryCoefs)
            {
                if (coef.Category == item.Type)
                {
                    sellCoef = coef.SellCoef;
                    break;
                }
            }

            return Mathf.Max(1, Mathf.RoundToInt(item.BasePrice * sellCoef));
        }
    }
}

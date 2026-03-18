using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.WorldModifiers;

namespace Internal.Scripts.Economy.Simulation
{
    public sealed class TradePriceModifiers
    {
        private readonly TradePriceSkillModifier _skillModifier;
        private readonly LanguagePriceModifier _languageModifier;
        private readonly ModifierEffectQuery _modifierQuery;
        private readonly EconomyDatabase _economyDb;
        private Dictionary<string, CultureId> _cityCultureCache;

        public TradePriceModifiers(
            TradePriceSkillModifier skillModifier,
            LanguagePriceModifier languageModifier,
            ModifierEffectQuery modifierQuery,
            EconomyDatabase economyDb)
        {
            _skillModifier = skillModifier;
            _languageModifier = languageModifier;
            _modifierQuery = modifierQuery;
            _economyDb = economyDb;
        }

        public float GetBuyMultiplier(string cityId)
        {
            return GetBuyBonusMultiplier(cityId) * GetWorldModifierMultiplier(cityId);
        }

        public float GetSellMultiplier(string cityId)
        {
            return GetSellBonusMultiplier(cityId) * GetWorldModifierMultiplier(cityId);
        }

        public float GetBuyBonusMultiplier(string cityId)
        {
            return _skillModifier.GetBuyMultiplier()
                * _languageModifier.GetBuyMultiplier(ResolveCulture(cityId));
        }

        public float GetSellBonusMultiplier(string cityId)
        {
            return _skillModifier.GetSellMultiplier()
                * _languageModifier.GetSellMultiplier(ResolveCulture(cityId));
        }

        public float GetWorldModifierMultiplier(string cityId)
        {
            return _modifierQuery.GetCityPriceMultiplier(cityId);
        }

        private CultureId ResolveCulture(string cityId)
        {
            if (_cityCultureCache == null)
            {
                _cityCultureCache = new Dictionary<string, CultureId>();
                foreach (CityData city in _economyDb.Cities)
                    _cityCultureCache[city.Id] = city.PrimaryCulture;
            }

            return _cityCultureCache.TryGetValue(cityId, out CultureId culture)
                ? culture
                : default;
        }
    }
}

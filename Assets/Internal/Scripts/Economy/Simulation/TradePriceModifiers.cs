using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Skills;

namespace Internal.Scripts.Economy.Simulation
{
    public sealed class TradePriceModifiers
    {
        private readonly TradePriceSkillModifier _skillModifier;
        private readonly LanguagePriceModifier _languageModifier;
        private readonly EconomyDatabase _economyDb;
        private Dictionary<string, CultureId> _cityCultureCache;

        public TradePriceModifiers(
            TradePriceSkillModifier skillModifier,
            LanguagePriceModifier languageModifier,
            EconomyDatabase economyDb)
        {
            _skillModifier = skillModifier;
            _languageModifier = languageModifier;
            _economyDb = economyDb;
        }

        public float GetBuyMultiplier(string cityId)
        {
            return _skillModifier.GetBuyMultiplier() * _languageModifier.GetBuyMultiplier(ResolveCulture(cityId));
        }

        public float GetSellMultiplier(string cityId)
        {
            return _skillModifier.GetSellMultiplier() * _languageModifier.GetSellMultiplier(ResolveCulture(cityId));
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

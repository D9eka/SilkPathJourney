using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Player.Languages.Generated;
using UnityEngine;

namespace Internal.Scripts.Player.Languages
{
    public sealed class LanguagePriceModifier
    {
        private readonly PlayerLanguageRepository _languageRepo;
        private readonly EconomyDatabase _economyDb;
        private Dictionary<CultureId, LanguageType> _cultureLanguageMap;

        private static readonly float[] BuyMods  = { 0.20f, 0.10f, 0f, -0.05f, -0.10f };
        private static readonly float[] SellMods = { -0.20f, -0.10f, 0f, 0.05f, 0.10f };

        public static float GetBuyMod(LanguageProficiency level) =>
            BuyMods[Mathf.Clamp((int)level, 0, BuyMods.Length - 1)];

        public static float GetSellMod(LanguageProficiency level) =>
            SellMods[Mathf.Clamp((int)level, 0, SellMods.Length - 1)];
        private const float SogdianBuyDiscount = -0.10f;

        public LanguagePriceModifier(PlayerLanguageRepository languageRepo, EconomyDatabase economyDb)
        {
            _languageRepo = languageRepo;
            _economyDb = economyDb;
        }

        public float GetBuyMultiplier(CultureId culture)
        {
            EnsureMap();
            float localMod = GetLocalBuyMod(culture);
            float sogdianMod = GetSogdianBuyMod();
            return 1f + Mathf.Min(localMod, sogdianMod);
        }

        public float GetSellMultiplier(CultureId culture)
        {
            EnsureMap();
            return 1f + GetModForCulture(culture, SellMods);
        }

        private float GetLocalBuyMod(CultureId culture)
        {
            return GetModForCulture(culture, BuyMods);
        }

        private float GetModForCulture(CultureId culture, float[] mods)
        {
            if (!_cultureLanguageMap.TryGetValue(culture, out LanguageType lang))
                return mods[0];

            int idx = (int)_languageRepo.Current.GetProficiency(lang);
            return mods[Mathf.Clamp(idx, 0, mods.Length - 1)];
        }

        private float GetSogdianBuyMod()
        {
            LanguageProficiency sogdian = _languageRepo.Current.GetProficiency(LanguageType.Sogdian);
            return sogdian >= LanguageProficiency.Basic ? SogdianBuyDiscount : BuyMods[0];
        }

        private void EnsureMap()
        {
            if (_cultureLanguageMap != null) return;

            _cultureLanguageMap = new Dictionary<CultureId, LanguageType>();
            foreach (EconomyDatabase.CultureLanguageMapping mapping in _economyDb.CultureLanguages)
                _cultureLanguageMap[mapping.Culture] = mapping.Language;
        }
    }
}

using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Player.Languages.Generated;
using UnityEngine;

namespace Internal.Scripts.Player.Languages
{
    public sealed class LanguagePriceModifier
    {
        private readonly PlayerLanguageRepository _languageRepo;
        private readonly PlayerResourceRepository _resourceRepo;
        private readonly CaravanDatabase _caravanDb;
        private readonly EconomyDatabase _economyDb;
        private Dictionary<CultureId, LanguageType> _cultureLanguageMap;

        private static readonly float[] BuyMods  = { 0.20f, 0.10f, 0f, -0.05f, -0.10f };
        private static readonly float[] SellMods = { -0.20f, -0.10f, 0f, 0.05f, 0.10f };

        public static float GetBuyMod(LanguageProficiency level) =>
            BuyMods[Mathf.Clamp((int)level, 0, BuyMods.Length - 1)];

        public static float GetSellMod(LanguageProficiency level) =>
            SellMods[Mathf.Clamp((int)level, 0, SellMods.Length - 1)];
        private const float SogdianBuyDiscount = -0.10f;

        public LanguagePriceModifier(PlayerLanguageRepository languageRepo,
            PlayerResourceRepository resourceRepo, CaravanDatabase caravanDb, EconomyDatabase economyDb)
        {
            _languageRepo = languageRepo;
            _resourceRepo = resourceRepo;
            _caravanDb = caravanDb;
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

            LanguageProficiency playerLevel = _languageRepo.Current.GetProficiency(lang);
            LanguageProficiency translatorLevel = GetTranslatorProficiency(lang);
            int effective = Mathf.Max((int)playerLevel, (int)translatorLevel);
            return mods[Mathf.Clamp(effective, 0, mods.Length - 1)];
        }

        private float GetSogdianBuyMod()
        {
            LanguageProficiency playerSogdian = _languageRepo.Current.GetProficiency(LanguageType.Sogdian);
            LanguageProficiency translatorSogdian = GetTranslatorProficiency(LanguageType.Sogdian);
            LanguageProficiency effective = (LanguageProficiency)Mathf.Max((int)playerSogdian, (int)translatorSogdian);
            return effective >= LanguageProficiency.Basic ? SogdianBuyDiscount : BuyMods[0];
        }

        private LanguageProficiency GetTranslatorProficiency(LanguageType targetLang)
        {
            var companions = _resourceRepo.Current.Companions;
            if (companions == null || companions.Count == 0)
                return LanguageProficiency.None;

            LanguageProficiency best = LanguageProficiency.None;
            string translatorId = CompanionType.Translator.ToString();

            foreach (CompanionState comp in companions)
            {
                if (comp.IsInjured)
                    continue;

                if (!string.Equals(comp.TypeId, translatorId, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (string.IsNullOrEmpty(comp.LanguageId))
                    continue;

                if (!Enum.TryParse(comp.LanguageId, true, out LanguageType compLang) || compLang != targetLang)
                    continue;

                if (!Enum.TryParse(comp.QualityId, true, out CompanionQuality quality))
                    continue;

                var bonus = _caravanDb.GetCompanionBonus(CompanionType.Translator, quality);
                int profValue = Mathf.RoundToInt(bonus.BonusValue);
                if (profValue > (int)best)
                    best = (LanguageProficiency)profValue;
            }

            return best;
        }

        public LanguageProficiency GetEffectiveProficiency(LanguageType lang)
        {
            LanguageProficiency playerLevel = _languageRepo.Current.GetProficiency(lang);
            LanguageProficiency translatorLevel = GetTranslatorProficiency(lang);
            return (LanguageProficiency)Mathf.Max((int)playerLevel, (int)translatorLevel);
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

using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Npc.Names;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.UI.Localization;
using UnityEngine;

namespace Internal.Scripts.Player
{
    public sealed class CompanionService
    {
        private readonly PlayerResourceRepository _resourceRepo;
        private readonly CaravanDatabase _caravanDb;
        private readonly EconomyDatabase _economyDb;
        private readonly NameDatabase _nameDb;
        private Dictionary<CultureId, LanguageType> _cultureLanguageMap;

        public CompanionService(PlayerResourceRepository resourceRepo, CaravanDatabase caravanDb,
            EconomyDatabase economyDb, NameDatabase nameDb)
        {
            _resourceRepo = resourceRepo;
            _caravanDb = caravanDb;
            _economyDb = economyDb;
            _nameDb = nameDb;
        }

        public event Action CompanionHired;
        public event Action CompanionRemoved;

        public int GetHireCost(CompanionType type, CompanionQuality quality, float regionMultiplier = 1f)
        {
            CompanionTypeData typeData = _caravanDb.GetCompanionType(type);
            CaravanDatabase.CompanionQualityEntry qualityData = _caravanDb.GetCompanionQuality(quality);
            if (typeData == null) return 0;
            return Mathf.RoundToInt(typeData.HireCostBase * qualityData.PriceMultiplier * regionMultiplier);
        }

        public bool HireCompanion(CompanionType type, CompanionQuality quality, int cost,
            CultureId culture = CultureId.None)
        {
            LanguageType language = LanguageType.None;
            if (type == CompanionType.Translator && culture != CultureId.None)
                language = GetLanguageForCulture(culture);

            return HireCompanionInternal(type, quality, culture, language, cost);
        }

        public bool HireCompanionWithLanguage(CompanionType type, CompanionQuality quality, int cost,
            LanguageType language)
        {
            return HireCompanionInternal(type, quality, CultureId.None, language, cost);
        }

        private bool HireCompanionInternal(CompanionType type, CompanionQuality quality,
            CultureId culture, LanguageType language, int cost)
        {
            var state = _resourceRepo.Current;
            if (state.Companions.Count >= state.MaxCompanions)
                return false;

            CompanionTypeData typeData = _caravanDb.GetCompanionType(type);
            if (typeData == null)
                return false;

            if (state.Money < cost)
                return false;

            NameEntry nameEntry = culture != CultureId.None
                ? _nameDb.GetRandom(culture)
                : _nameDb.GetRandom();
            string name = nameEntry?.Name != null
                ? LocalizationService.ResolveString(nameEntry.Name, nameEntry.Id, "CompanionService.Name")
                : typeData.Id;

            string languageId = type == CompanionType.Translator && language != LanguageType.None
                ? language.ToString()
                : null;

            _resourceRepo.UpdateResources(s =>
            {
                s.Money -= cost;
                s.Companions.Add(new CompanionState
                {
                    TypeId = typeData.Id,
                    QualityId = quality.ToString(),
                    Name = name,
                    IsInjured = false,
                    LanguageId = languageId
                });
            });

            CompanionHired?.Invoke();
            return true;
        }

        public LanguageType GetLanguageForCulture(CultureId culture)
        {
            EnsureCultureLanguageMap();
            return _cultureLanguageMap.TryGetValue(culture, out LanguageType lang)
                ? lang
                : LanguageType.None;
        }

        private void EnsureCultureLanguageMap()
        {
            if (_cultureLanguageMap != null) return;

            _cultureLanguageMap = new Dictionary<CultureId, LanguageType>();
            foreach (EconomyDatabase.CultureLanguageMapping mapping in _economyDb.CultureLanguages)
                _cultureLanguageMap[mapping.Culture] = mapping.Language;
        }

        public void RemoveCompanion(int index)
        {
            _resourceRepo.UpdateResources(s =>
            {
                if (index >= 0 && index < s.Companions.Count)
                    s.Companions.RemoveAt(index);
            });
            CompanionRemoved?.Invoke();
        }

        public void HealCompanion(int index)
        {
            _resourceRepo.UpdateResources(s =>
            {
                if (index >= 0 && index < s.Companions.Count)
                    s.Companions[index].IsInjured = false;
            });
        }
    }
}

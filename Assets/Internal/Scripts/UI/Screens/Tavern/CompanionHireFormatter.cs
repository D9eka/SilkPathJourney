using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Npc.Names;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.UI.Localization;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Tavern
{
    public sealed class CompanionHireFormatter
    {
        private readonly CaravanDatabase _caravanDb;
        private readonly EconomyDatabase _economyDb;
        private readonly NameDatabase _nameDb;
        private readonly CompanionService _companionService;

        public CompanionHireFormatter(
            CaravanDatabase caravanDb,
            EconomyDatabase economyDb,
            NameDatabase nameDb,
            CompanionService companionService)
        {
            _caravanDb = caravanDb;
            _economyDb = economyDb;
            _nameDb = nameDb;
            _companionService = companionService;
        }

        public List<CompanionHireData> Build(
            CultureId cityCulture,
            int playerMoney,
            int currentCompanionCount,
            int maxCompanions,
            List<(CompanionType type, CompanionQuality quality)> slotsOut)
        {
            slotsOut.Clear();
            var hireList = new List<CompanionHireData>();
            int index = 0;
            string effectPrefix = ResolveLoc("UI.Global.Effect.Prefix", "UI.Global.Effect.Prefix");
            string levelPrefix = ResolveLoc("UI.Global.Level.Prefix", "UI.Global.Level.Prefix");

            foreach (var typeData in _caravanDb.CompanionTypes)
            {
                foreach (var qualityEntry in _caravanDb.CompanionQualities)
                {
                    var entry = FormatEntry(typeData, qualityEntry, cityCulture,
                        playerMoney, currentCompanionCount, maxCompanions,
                        effectPrefix, levelPrefix, index);

                    slotsOut.Add((typeData.Type, qualityEntry.Quality));
                    hireList.Add(entry);
                    index++;
                }
            }

            return hireList;
        }

        private CompanionHireData FormatEntry(
            CompanionTypeData typeData, CaravanDatabase.CompanionQualityEntry qualityEntry,
            CultureId cityCulture, int playerMoney, int currentCount, int maxCompanions,
            string effectPrefix, string levelPrefix, int index)
        {
            int hireCost = _companionService.GetHireCost(typeData.Type, qualityEntry.Quality);
            int dailyCost = Mathf.RoundToInt(typeData.DailyCostBase * qualityEntry.DailyCostMultiplier);

            string displayName = FormatDisplayName(typeData, cityCulture);
            string qualityName = FormatQualityName(qualityEntry, levelPrefix);
            string effectText = $"{effectPrefix} {FormatEffect(typeData, qualityEntry, cityCulture)}";
            bool canHire = playerMoney >= hireCost && currentCount < maxCompanions;

            return new CompanionHireData(
                displayName, qualityName, effectText,
                ResolveLoc("UI.Tavern.HireCost", "UI.Tavern.HireCost", hireCost),
                ResolveLoc("UI.Tavern.DailyCost", "UI.Tavern.DailyCost", dailyCost),
                canHire, index, typeData.Type, qualityEntry.Quality);
        }

        private string FormatDisplayName(CompanionTypeData typeData, CultureId cityCulture)
        {
            string typeName = LocalizationService.ResolveString(
                typeData.Name, typeData.Id, "Tavern.CompanionType");

            var nameEntry = _nameDb.GetRandom(cityCulture);
            string companionName = nameEntry?.Name != null
                ? LocalizationService.ResolveString(nameEntry.Name, nameEntry.Id, "Tavern.Name")
                : "";

            return string.IsNullOrEmpty(companionName) ? typeName : $"{typeName} {companionName}";
        }

        private static string FormatQualityName(CaravanDatabase.CompanionQualityEntry qualityEntry, string levelPrefix)
        {
            string rawQuality = LocalizationService.ResolveString(
                qualityEntry.Name, qualityEntry.Quality.ToString(), "Tavern.CompanionQuality");
            return $"{levelPrefix} {rawQuality}";
        }

        private string FormatEffect(CompanionTypeData typeData, CaravanDatabase.CompanionQualityEntry qualityEntry, CultureId cityCulture)
        {
            var bonus = _caravanDb.GetCompanionBonus(typeData.Type, qualityEntry.Quality);

            if (typeData.Type == CompanionType.Translator)
                return FormatTranslatorEffect(cityCulture, bonus);

            return bonus.Description != null
                ? LocalizationService.ResolveString(bonus.Description, bonus.BonusKey, "Tavern.Bonus")
                : bonus.BonusKey ?? "";
        }

        private string FormatTranslatorEffect(CultureId cityCulture, CaravanDatabase.CompanionBonusEntry bonus)
        {
            LanguageType cityLang = _economyDb.GetLanguageForCulture(cityCulture);
            string langName = ResolveLanguageName(cityLang);
            int profValue = Mathf.RoundToInt(bonus.BonusValue);
            string profName = ResolveProficiencyName((LanguageProficiency)profValue);
            return ResolveLoc("UI.Translator.LanguageEffect",
                "UI.Translator.LanguageEffect", langName, profName);
        }

        private static string ResolveLanguageName(LanguageType lang)
        {
            if (lang == LanguageType.None)
                return "";

            var localized = new LocalizedString("UI", $"UI.Language.{lang}.Name");
            return LocalizationService.ResolveString(localized, lang.ToString(), $"LanguageName.{lang}");
        }

        private static string ResolveProficiencyName(LanguageProficiency proficiency)
        {
            var localized = new LocalizedString("UI", $"UI.Language.Proficiency.{proficiency}");
            return LocalizationService.ResolveString(localized, proficiency.ToString(), $"Proficiency.{proficiency}");
        }

        private static string ResolveLoc(string key, string fallback, params object[] args)
        {
            var localized = new LocalizedString("UI", key);
            return LocalizationService.ResolveString(localized, fallback, key, args);
        }
    }
}

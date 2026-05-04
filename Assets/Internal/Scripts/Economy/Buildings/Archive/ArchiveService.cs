using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.UI.Localization;
using UnityEngine.Localization;

namespace Internal.Scripts.Economy.Buildings.Archive
{
    public sealed class ArchiveService
    {
        private readonly EconomyDatabase _economyDatabase;
        private readonly PlayerLanguageRepository _languageRepository;

        public ArchiveService(EconomyDatabase economyDatabase, PlayerLanguageRepository languageRepository)
        {
            _economyDatabase = economyDatabase;
            _languageRepository = languageRepository;
        }

        public IReadOnlyList<CityData> GetCities()
        {
            return _economyDatabase.Cities;
        }

        public IReadOnlyList<CultureRefEntry> GetCultures()
        {
            var values = (CultureId[])Enum.GetValues(typeof(CultureId));
            var result = new List<CultureRefEntry>(values.Length);
            foreach (var culture in values)
            {
                if (culture == CultureId.None) continue;
                string id = culture.ToString().ToLowerInvariant();
                string nameKey = $"culture.{id}.name";
                string descKey = $"culture.{id}.description";
                string name = LocalizationService.ResolveString(new LocalizedString("Economy", nameKey), id, "Archive.Culture");
                string description = LocalizationService.ResolveString(new LocalizedString("Economy", descKey), string.Empty, "Archive.Culture.Desc");
                var language = _economyDatabase.GetLanguageForCulture(culture);
                result.Add(new CultureRefEntry(culture, name, description, language));
            }
            return result;
        }

        public IReadOnlyList<LanguageRefEntry> GetLanguages()
        {
            var values = (LanguageType[])Enum.GetValues(typeof(LanguageType));
            var langState = _languageRepository.Current;
            var result = new List<LanguageRefEntry>(values.Length);
            foreach (var lang in values)
            {
                if (lang == LanguageType.None) continue;
                string nameKey = $"UI.Language.{lang}.Name";
                string name = LocalizationService.ResolveString(new LocalizedString("UI", nameKey), lang.ToString(), "Archive.Language");
                var proficiency = langState.GetProficiency(lang);
                result.Add(new LanguageRefEntry(lang, name, proficiency));
            }
            return result;
        }
    }

    public readonly struct CultureRefEntry
    {
        public readonly CultureId Culture;
        public readonly string Name;
        public readonly string Description;
        public readonly LanguageType Language;

        public CultureRefEntry(CultureId culture, string name, string description, LanguageType language)
        {
            Culture = culture;
            Name = name;
            Description = description;
            Language = language;
        }
    }

    public readonly struct LanguageRefEntry
    {
        public readonly LanguageType Language;
        public readonly string Name;
        public readonly LanguageProficiency Proficiency;

        public LanguageRefEntry(LanguageType language, string name, LanguageProficiency proficiency)
        {
            Language = language;
            Name = name;
            Proficiency = proficiency;
        }
    }
}

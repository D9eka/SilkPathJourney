using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Buildings.Archive;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.UI.Localization;

namespace Internal.Scripts.UI.Screens.Archive.Tabs
{
    internal sealed class CulturesArchiveTabBuilder : IArchiveTabBuilder
    {
        private readonly ArchiveService _archiveService;

        public ArchiveTab Tab => ArchiveTab.Cultures;

        public CulturesArchiveTabBuilder(ArchiveService archiveService)
        {
            _archiveService = archiveService;
        }

        public IReadOnlyList<ArchiveListEntry> BuildItems()
        {
            var cultures = _archiveService.GetCultures();
            var cities = _archiveService.GetCities();
            var result = new List<ArchiveListEntry>(cultures.Count);
            foreach (var entry in cultures)
            {
                int count = 0;
                foreach (var city in cities)
                    if (city.PrimaryCulture == entry.Culture) count++;

                string countText = LocalizationService.Resolve("UI", "ui.archive.cultures.city_count", $"{count}", count);
                string languageName = ResolveLanguageName(entry.Language);
                string description = string.IsNullOrEmpty(languageName)
                    ? countText
                    : countText + "\n" + LocalizationService.Resolve("UI", "ui.archive.cultures.language_label", $"Язык: {languageName}", languageName);

                result.Add(new ArchiveListEntry(entry.Culture.ToString(), entry.Name, description));
            }
            return result;
        }

        private static string ResolveLanguageName(LanguageType lang)
        {
            if (lang == LanguageType.None) return string.Empty;
            return LocalizationService.Resolve("UI", $"UI.Language.{lang}.Name", lang.ToString());
        }

        public ArchiveDetailData BuildDetail(string selectedId)
        {
            if (string.IsNullOrEmpty(selectedId))
                return ArchiveDetailData.Empty;
            if (!Enum.TryParse(selectedId, out CultureId culture))
                return ArchiveDetailData.Empty;

            string id = culture.ToString().ToLowerInvariant();
            string name = LocalizationService.Resolve("Economy", $"culture.{id}.name", id);
            string desc = LocalizationService.Resolve("Economy", $"culture.{id}.description", string.Empty);
            return new ArchiveDetailData(name, desc);
        }
    }
}

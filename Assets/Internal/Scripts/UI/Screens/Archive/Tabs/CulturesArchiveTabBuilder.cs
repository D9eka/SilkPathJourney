using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Buildings.Archive;
using Internal.Scripts.Economy.Generated;
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
                result.Add(new ArchiveListEntry(entry.Culture.ToString(), entry.Name, count.ToString()));
            }
            return result;
        }

        public ArchiveDetailData BuildDetail(string selectedId)
        {
            if (string.IsNullOrEmpty(selectedId))
                return ArchiveDetailData.Empty;
            if (!Enum.TryParse(selectedId, out CultureId culture))
                return ArchiveDetailData.Empty;

            string id = culture.ToString().ToLowerInvariant();
            string name = LocalizationService.Resolve("UI", $"culture.{id}.name", id);
            string desc = LocalizationService.Resolve("UI", $"culture.{id}.description", string.Empty);
            return new ArchiveDetailData(name, desc);
        }
    }
}

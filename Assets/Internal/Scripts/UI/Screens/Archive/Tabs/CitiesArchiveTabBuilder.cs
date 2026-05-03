using System.Collections.Generic;
using System.Text.RegularExpressions;
using Internal.Scripts.Economy.Buildings.Archive;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.UI.Localization;

namespace Internal.Scripts.UI.Screens.Archive.Tabs
{
    internal sealed class CitiesArchiveTabBuilder : IArchiveTabBuilder
    {
        private static readonly Regex PASCAL_TO_SNAKE = new Regex("(?<=[a-z])([A-Z])", RegexOptions.Compiled);

        private readonly ArchiveService _archiveService;

        public ArchiveTab Tab => ArchiveTab.Cities;

        public CitiesArchiveTabBuilder(ArchiveService archiveService)
        {
            _archiveService = archiveService;
        }

        public IReadOnlyList<ArchiveListEntry> BuildItems()
        {
            var cities = _archiveService.GetCities();
            var result = new List<ArchiveListEntry>(cities.Count);
            foreach (var city in cities)
            {
                string name = LocalizationService.ResolveString(city.Name, city.Id, "Archive.City");
                string culture = ResolveCultureName(city.PrimaryCulture);
                string cityType = ResolveCityType(city.Type);
                string description = string.IsNullOrEmpty(culture) ? cityType : $"{culture}\n{cityType}";
                result.Add(new ArchiveListEntry(city.Id, name, description));
            }
            return result;
        }

        public ArchiveDetailData BuildDetail(string selectedId)
        {
            if (string.IsNullOrEmpty(selectedId))
                return ArchiveDetailData.Empty;

            CityData found = null;
            foreach (var city in _archiveService.GetCities())
            {
                if (city.Id == selectedId) { found = city; break; }
            }
            if (found == null) return ArchiveDetailData.Empty;

            string name = LocalizationService.ResolveString(found.Name, found.Id, "Archive.CityDetail.Name");
            string desc = LocalizationService.ResolveString(found.Description, string.Empty, "Archive.CityDetail.Desc");
            return new ArchiveDetailData(name, desc);
        }

        private static string ResolveCultureName(CultureId culture)
        {
            if (culture == CultureId.None) return string.Empty;
            string id = culture.ToString().ToLowerInvariant();
            return LocalizationService.Resolve("Economy", $"culture.{id}.name", culture.ToString());
        }

        private static string ResolveCityType(CityType type)
        {
            if (type == CityType.Unknown) return string.Empty;
            string id = PASCAL_TO_SNAKE.Replace(type.ToString(), "_$1").ToLowerInvariant();
            return LocalizationService.Resolve("Economy", $"city_type.{id}.name", type.ToString());
        }
    }
}

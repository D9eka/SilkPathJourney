using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Localization;

namespace Internal.Scripts.Trading
{
    public sealed class TradeCityCatalog
    {
        private readonly Dictionary<string, CityData> _citiesById = new();

        public TradeCityCatalog(EconomyDatabase economyDatabase)
        {
            if (economyDatabase == null || economyDatabase.Cities == null)
                return;

            foreach (CityData city in economyDatabase.Cities)
            {
                if (city == null || string.IsNullOrWhiteSpace(city.Id))
                    continue;

                _citiesById.TryAdd(city.Id, city);
            }
        }

        public string ResolveCityName(string cityId)
        {
            if (string.IsNullOrWhiteSpace(cityId))
                return string.Empty;

            if (_citiesById.TryGetValue(cityId, out CityData city) && city != null)
                return LocalizedStringResolver.Resolve(city.Name, city.Id, $"City.{city.Id}.Name");

            return cityId;
        }
    }
}

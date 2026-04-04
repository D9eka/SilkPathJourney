using System;
using Internal.Scripts.Economy.Cities.Smuggling;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Economy.Cities.Tariff
{
    public sealed class CityEntryTaxService : IInitializable, IDisposable
    {
        private readonly ICityEntryService _cityEntry;
        private readonly TariffService _tariffService;
        private readonly SmugglingCheckService _smugglingService;

        public CityEntryTaxService(
            ICityEntryService cityEntry,
            TariffService tariffService,
            SmugglingCheckService smugglingService)
        {
            _cityEntry = cityEntry;
            _tariffService = tariffService;
            _smugglingService = smugglingService;
        }

        public void Initialize()
        {
            _cityEntry.OnCityEntered += HandleCityEntered;
        }

        public void Dispose()
        {
            _cityEntry.OnCityEntered -= HandleCityEntered;
        }

        private void HandleCityEntered(CityData city)
        {
            var tariffResult = _tariffService.ChargePlayerTariff(city);
            if (tariffResult.WasCharged)
                Debug.Log($"[Tariff] Charged {tariffResult.Amount} at {city.Id}");

            var smugglingResult = _smugglingService.PerformCheck(city);
            if (smugglingResult.WasCaught)
                Debug.Log($"[Smuggling] Caught at {city.Id}! Penalty: {smugglingResult.Penalty}");
        }
    }
}

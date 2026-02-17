using System;
using Internal.Scripts.Economy.Cities;
using Zenject;

namespace Internal.Scripts.World.State
{
    public class WorldStateController : IInitializable, IDisposable
    {
        public Action<WorldViewMode> OnStateChange;

        private readonly ICityEntryService _cityEntryService;

        private WorldViewMode _currentViewMode = WorldViewMode.Strategic;

        public WorldViewMode CurrentViewMode => _currentViewMode;

        public WorldStateController(ICityEntryService cityEntryService)
        {
            _cityEntryService = cityEntryService;
        }

        public void Initialize()
        {
            _cityEntryService.OnCityEntered += HandleCityEntered;
            _cityEntryService.OnCityExited += HandleCityExited;
        }

        public void Dispose()
        {
            _cityEntryService.OnCityEntered -= HandleCityEntered;
            _cityEntryService.OnCityExited -= HandleCityExited;
        }

        private void HandleCityEntered(CityData _) => SetViewMode(WorldViewMode.Detailed);
        private void HandleCityExited() => SetViewMode(WorldViewMode.Strategic);

        private void SetViewMode(WorldViewMode mode)
        {
            if (mode == _currentViewMode) return;
            _currentViewMode = mode;
            OnStateChange?.Invoke(_currentViewMode);
        }
    }
}

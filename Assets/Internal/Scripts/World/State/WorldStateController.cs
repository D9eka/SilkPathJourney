using System;
using Internal.Scripts.Economy.Cities;
using Zenject;

namespace Internal.Scripts.World.State
{
    public class WorldStateController : IInitializable, IDisposable
    {
        public Action<WorldViewMode> OnStateChange;

        private readonly ICityEntryService _cityEntryService;
        private readonly GameClock _gameClock;

        private WorldViewMode _currentViewMode = WorldViewMode.Strategic;
        private TimeSpeed _speedBeforeDetailView;

        public WorldViewMode CurrentViewMode => _currentViewMode;

        public WorldStateController(ICityEntryService cityEntryService, GameClock gameClock)
        {
            _cityEntryService = cityEntryService;
            _gameClock = gameClock;
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

        private void HandleCityEntered(CityData _)
        {
            _speedBeforeDetailView = _gameClock.SelectedSpeed.Value;
            _gameClock.SetSelectedSpeed(TimeSpeed.Paused);
            SetViewMode(WorldViewMode.Detailed);
        }

        private void HandleCityExited()
        {
            if (_speedBeforeDetailView != TimeSpeed.Paused)
                _gameClock.SetSelectedSpeed(TimeSpeed.Normal);
            SetViewMode(WorldViewMode.Strategic);
        }

        private void SetViewMode(WorldViewMode mode)
        {
            if (mode == _currentViewMode) return;
            _currentViewMode = mode;
            OnStateChange?.Invoke(_currentViewMode);
        }
    }
}

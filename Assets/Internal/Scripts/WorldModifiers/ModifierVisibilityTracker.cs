using System;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events;
using Internal.Scripts.Player;
using Zenject;

namespace Internal.Scripts.WorldModifiers
{
    public sealed class ModifierVisibilityTracker : IInitializable, IDisposable
    {
        private readonly IPlayerStateEvents _playerEvents;
        private readonly ICityNodeResolver _cityResolver;
        private readonly CurrentRoadResolver _roadResolver;
        private readonly WorldModifierRepository _repo;
        private readonly DayTracker _dayTracker;

        public ModifierVisibilityTracker(
            IPlayerStateEvents playerEvents,
            ICityNodeResolver cityResolver,
            CurrentRoadResolver roadResolver,
            WorldModifierRepository repo,
            DayTracker dayTracker)
        {
            _playerEvents = playerEvents;
            _cityResolver = cityResolver;
            _roadResolver = roadResolver;
            _repo = repo;
            _dayTracker = dayTracker;
        }

        public void Initialize()
        {
            _playerEvents.OnCurrentNodeChanged += HandleNodeChanged;
        }

        public void Dispose()
        {
            _playerEvents.OnCurrentNodeChanged -= HandleNodeChanged;
        }

        private void HandleNodeChanged(string nodeId)
        {
            int day = _dayTracker.CurrentDay;

            if (_cityResolver.TryGetCityByNodeId(nodeId, out CityData city))
                _repo.MarkCitySeen(city.Id, day);

            string roadId = _roadResolver.GetCurrentRoadId();
            if (roadId != null)
                _repo.MarkRoadSeen(roadId, day);
        }
    }
}

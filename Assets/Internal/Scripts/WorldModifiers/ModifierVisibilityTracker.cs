using System;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events;
using Internal.Scripts.Player;
using R3;
using Zenject;

namespace Internal.Scripts.WorldModifiers
{
    public sealed class ModifierVisibilityTracker : IInitializable, IDisposable
    {
        private readonly IPlayerStateEvents _playerEvents;
        private readonly IPlayerStateProvider _playerState;
        private readonly ICityNodeResolver _cityResolver;
        private readonly CurrentRoadResolver _roadResolver;
        private readonly WorldModifierRepository _repo;
        private readonly DayTracker _dayTracker;
        private IDisposable _repoSubscription;
        private bool _isHandlingRepoChange;

        public ModifierVisibilityTracker(
            IPlayerStateEvents playerEvents,
            IPlayerStateProvider playerState,
            ICityNodeResolver cityResolver,
            CurrentRoadResolver roadResolver,
            WorldModifierRepository repo,
            DayTracker dayTracker)
        {
            _playerEvents = playerEvents;
            _playerState = playerState;
            _cityResolver = cityResolver;
            _roadResolver = roadResolver;
            _repo = repo;
            _dayTracker = dayTracker;
        }

        public void Initialize()
        {
            _playerEvents.OnCurrentNodeChanged += HandleNodeChanged;
            _playerEvents.OnCurrentSegmentChanged += HandleSegmentChanged;
            _repoSubscription = _repo.Changed.Subscribe(_ => HandleRepoChanged());
            RevealCurrentLocation();
        }

        public void Dispose()
        {
            _playerEvents.OnCurrentNodeChanged -= HandleNodeChanged;
            _playerEvents.OnCurrentSegmentChanged -= HandleSegmentChanged;
            _repoSubscription?.Dispose();
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

        private void HandleSegmentChanged(string fromNode, string toNode)
        {
            string roadId = _roadResolver.GetCurrentRoadId();
            if (roadId != null)
                _repo.MarkRoadSeen(roadId, _dayTracker.CurrentDay);
        }

        private void HandleRepoChanged()
        {
            if (_isHandlingRepoChange) return;
            _isHandlingRepoChange = true;
            try
            {
                RevealCurrentLocation();
            }
            finally
            {
                _isHandlingRepoChange = false;
            }
        }

        private void RevealCurrentLocation()
        {
            int day = _dayTracker.CurrentDay;

            string roadId = _roadResolver.GetCurrentRoadId();
            if (roadId != null)
                _repo.MarkRoadSeen(roadId, day);

            string nodeId = _playerState.CurrentNodeId;
            if (!string.IsNullOrEmpty(nodeId) && _cityResolver.TryGetCityByNodeId(nodeId, out CityData city))
                _repo.MarkCitySeen(city.Id, day);
        }
    }
}

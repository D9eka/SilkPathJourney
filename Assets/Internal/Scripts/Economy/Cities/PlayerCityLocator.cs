using Internal.Scripts.Player;
using Internal.Scripts.Road.Nodes;

namespace Internal.Scripts.Economy.Cities
{
    public class PlayerCityLocator
    {
        private readonly ICityEntryService _cityEntryService;
        private readonly IPlayerStateProvider _playerState;
        private readonly IRoadNodeLookup _nodeLookup;
        private readonly ICityNodeResolver _cityNodeResolver;

        public PlayerCityLocator(
            ICityEntryService cityEntryService,
            IPlayerStateProvider playerState,
            IRoadNodeLookup nodeLookup,
            ICityNodeResolver cityNodeResolver)
        {
            _cityEntryService = cityEntryService;
            _playerState = playerState;
            _nodeLookup = nodeLookup;
            _cityNodeResolver = cityNodeResolver;
        }

        public string ResolveCurrentCityId()
        {
            var current = _cityEntryService.CurrentCity;
            if (current != null) return current.Id;

            string nearestNode = _nodeLookup.FindNearestNodeId(_playerState.CurrentPosition);
            if (!string.IsNullOrEmpty(nearestNode) && _cityNodeResolver.TryGetCityByNodeId(nearestNode, out var nearCity))
                return nearCity.Id;

            return string.Empty;
        }
    }
}

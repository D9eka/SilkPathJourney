using Internal.Scripts.Player;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.WorldModifiers
{
    public sealed class CurrentRoadResolver
    {
        private readonly IPlayerStateProvider _playerState;
        private readonly IRoadNetwork _network;

        public CurrentRoadResolver(IPlayerStateProvider playerState, IRoadNetwork network)
        {
            _playerState = playerState;
            _network = network;
        }

        public string GetCurrentRoadId()
        {
            string fromNode = _playerState.CurrentFromNodeId;
            string toNode = _playerState.CurrentToNodeId;
            if (string.IsNullOrEmpty(fromNode) || string.IsNullOrEmpty(toNode))
                return null;

            return _network.TryGetSegment(fromNode, toNode, out RoadPathSegment segment)
                ? segment.SegmentId.RoadId
                : null;
        }
    }
}

using Internal.Scripts.Player;
using Internal.Scripts.Road.Core;

namespace Internal.Scripts.WorldModifiers
{
    public sealed class CurrentRoadResolver
    {
        private readonly IPlayerStateProvider _playerState;
        private readonly RoadRuntime[] _roads;

        public CurrentRoadResolver(IPlayerStateProvider playerState, RoadRuntime[] roads)
        {
            _playerState = playerState;
            _roads = roads;
        }

        public string GetCurrentRoadId()
        {
            string fromNode = _playerState.CurrentFromNodeId;
            string toNode = _playerState.CurrentToNodeId;
            if (string.IsNullOrEmpty(fromNode) || string.IsNullOrEmpty(toNode))
                return null;

            foreach (var road in _roads)
            {
                if (road?.Data == null) continue;
                var data = road.Data;
                if ((data.StartNodeId == fromNode && data.EndNodeId == toNode) ||
                    (data.Bidirectional && data.StartNodeId == toNode && data.EndNodeId == fromNode))
                {
                    return data.RoadId;
                }
            }
            return null;
        }
    }
}

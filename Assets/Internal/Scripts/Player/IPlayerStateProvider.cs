using UnityEngine;

namespace Internal.Scripts.Player
{
    public interface IPlayerStateProvider
    {
        PlayerState State { get; }
        string CurrentNodeId { get; }
        string StationaryNodeId { get; }
        string DestinationNodeId { get; }
        string CurrentFromNodeId { get; }
        string CurrentToNodeId { get; }
        Vector3 CurrentPosition { get; }
    }
}

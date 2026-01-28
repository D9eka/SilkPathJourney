using Internal.Scripts.Npc.Core;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.Player.UI.Arrow.JunctionBalancer
{
    public interface IArrowJunctionBalancer
    {
        void Initialize(RoadAgent playerRoadAgent);
        Vector3 GetBalancedPosition(Vector3 basePos, PathGroup group, RoadPathSegment segment);
        (PathGroup group, float angle) GetPathClassification(RoadPathSegment segment);
    }
}

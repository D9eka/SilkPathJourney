using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.Player.UI.Arrow.PositionCalculation
{
    public interface IArrowPositionCalculator
    {
        Vector3 CalculateWorldPosition(RoadPathSegment segment, float distanceAlongSegment, RoadLane lane);
    }
}
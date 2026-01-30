using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.UI.Arrow.PositionCalculation
{
    public interface IArrowPositionCalculator
    {
        Vector3 CalculateWorldPosition(RoadPathSegment segment, RoadLane lane);
        Vector3 SnapToGround(Vector3 position);
    }
}
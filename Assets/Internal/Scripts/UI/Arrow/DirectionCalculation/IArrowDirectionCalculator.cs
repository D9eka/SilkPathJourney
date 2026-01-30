using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.UI.Arrow.DirectionCalculation
{
    public interface IArrowDirectionCalculator
    {
        Vector3 CalculateWorldDirection(RoadPathSegment segment, float distanceAlongSegment);
    }
}
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.Road.Positioning
{
    public interface IRoadSidePositionCalculator
    {
        /// <summary>
        /// Calculates position alongside a road segment with configurable parameters.
        /// </summary>
        /// <param name="segment">The road path segment to position alongside</param>
        /// <param name="lane">The lane to use for positioning</param>
        /// <param name="distanceAlongPct">Distance along the segment as percentage (0.0 to 1.0)</param>
        /// <param name="lateralOffsetPct">Lateral offset as percentage of road width</param>
        /// <param name="heightAboveGround">Height above ground level</param>
        /// <returns>World position calculated for the given parameters</returns>
        Vector3 CalculatePosition(
            RoadPathSegment segment,
            RoadLane lane,
            float distanceAlongPct,
            float lateralOffsetPct,
            float heightAboveGround);

        /// <summary>
        /// Snaps a position to the ground with the specified height offset.
        /// </summary>
        /// <param name="position">The position to snap</param>
        /// <param name="height">Height above ground after snapping</param>
        /// <returns>Ground-snapped position</returns>
        Vector3 SnapToGround(Vector3 position, float height);
    }
}

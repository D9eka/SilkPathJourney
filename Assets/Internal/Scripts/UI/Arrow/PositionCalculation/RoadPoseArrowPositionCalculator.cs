using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using Internal.Scripts.Road.Positioning;
using UnityEngine;

namespace Internal.Scripts.UI.Arrow.PositionCalculation
{
    public sealed class RoadPoseArrowPositionCalculator : IArrowPositionCalculator
    {
        private const float DISTANCE_ALONG_PCT = 0.15f;
        private const float LANE_OFFSET_PCT = 0.2857143f;
        private const float GROUND_SNAP_HEIGHT_PCT = 1.0f;
        private const float GROUND_SNAP_HEIGHT = 1.5f;
        private const float MIN_GROUND_SNAP_HEIGHT = 1.0f;

        private readonly IRoadSidePositionCalculator _roadSidePositionCalculator;
        private readonly IRoadNetwork _roadNetwork;

        public RoadPoseArrowPositionCalculator(
            IRoadSidePositionCalculator roadSidePositionCalculator,
            IRoadNetwork roadNetwork)
        {
            _roadSidePositionCalculator = roadSidePositionCalculator;
            _roadNetwork = roadNetwork;
        }

        public Vector3 CalculateWorldPosition(RoadPathSegment segment, RoadLane lane)
        {
            if (!_roadNetwork.TryGetSegment(segment.SegmentId, out RoadSegmentData segmentData))
            {
                Debug.LogError($"[ArrowPositionCalculator] Segment not found: {segment.SegmentId}");
                return Vector3.zero;
            }

            float roadWidth = segmentData.Data != null
                ? segmentData.Data.LaneWidth * segmentData.Data.LaneCount
                : 5f;

            float heightAboveGround = Mathf.Max(roadWidth * GROUND_SNAP_HEIGHT_PCT, MIN_GROUND_SNAP_HEIGHT);

            return _roadSidePositionCalculator.CalculatePosition(
                segment,
                lane,
                DISTANCE_ALONG_PCT,
                LANE_OFFSET_PCT,
                heightAboveGround);
        }

        public Vector3 SnapToGround(Vector3 position)
        {
            return _roadSidePositionCalculator.SnapToGround(position, GROUND_SNAP_HEIGHT);
        }
    }
}

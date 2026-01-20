using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.Player.UI.Arrow.PositionCalculation
{
    public sealed class RoadPoseArrowPositionCalculator : IArrowPositionCalculator
    {
        private const float ARROW_DISTANCE_OFFSET = 5f;
        private const float LANE_OFFSET = 2f;
        private const float GROUND_SNAP_HEIGHT = 1.5f;

        private readonly IRoadNetwork _roadNetwork;
        private readonly RoadSamplerCache _samplerCache;
        private readonly RoadPoseSampler _poseSampler;
        private readonly GroundSnapper _groundSnapper;

        public RoadPoseArrowPositionCalculator(
            IRoadNetwork roadNetwork,
            RoadSamplerCache samplerCache,
            RoadPoseSampler poseSampler,
            GroundSnapper groundSnapper)
        {
            _roadNetwork = roadNetwork;
            _samplerCache = samplerCache;
            _poseSampler = poseSampler;
            _groundSnapper = groundSnapper;
        }

        public Vector3 CalculateWorldPosition(RoadPathSegment segment, RoadLane lane)
        {
            if (!_roadNetwork.TryGetSegment(segment.SegmentId, out RoadSegmentData segmentData))
            {
                Debug.LogError($"[ArrowPositionCalculator] Segment not found: {segment.SegmentId}");
                return Vector3.zero;
            }
            if (!_samplerCache.TryGetSampler(segmentData.Runtime, out RoadPolylineSampler sampler))
            {
                Debug.LogError($"[ArrowPositionCalculator] Sampler not found for road");
                return Vector3.zero;
            }

            float distanceAlongPolyline = CalculateDistanceAlongPolyline(segment, segmentData);
            float lateralOffset = (int)lane * LANE_OFFSET;
            
            RoadPose roadPose = _poseSampler.Sample(
                sampler,
                distanceAlongPolyline,
                segmentData.Runtime.WorldRoot,
                segmentData.Runtime.transform,
                segmentData.Data,
                lane,
                lateralOffset,
                segment.IsForward
            );
            
            return _groundSnapper.SnapToGround(roadPose.Position, GROUND_SNAP_HEIGHT);
        }

        public Vector3 SnapToGround(Vector3 position)
        {
            return _groundSnapper.SnapToGround(position, GROUND_SNAP_HEIGHT);
        }

        private float CalculateDistanceAlongPolyline(
            RoadPathSegment segment,
            RoadSegmentData segmentData)
        {
            float clampedDistance = Mathf.Min(ARROW_DISTANCE_OFFSET, segmentData.LengthMeters * 0.9f);
            return segment.IsForward
                ? clampedDistance
                : segmentData.LengthMeters - clampedDistance;
        }
    }
}
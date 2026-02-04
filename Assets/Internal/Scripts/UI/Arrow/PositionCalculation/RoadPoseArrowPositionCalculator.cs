using System.Collections.Generic;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
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

        private readonly HashSet<RoadSegmentId> _badSegments = new HashSet<RoadSegmentId>();

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

            if (!TryGetRoadWidth(segment, segmentData, out float roadWidth))
                return Vector3.zero;

            float distanceAlongPolyline = CalculateDistanceAlongPolyline(segment, segmentData, roadWidth);
            float lateralOffset = (int)lane * (roadWidth * LANE_OFFSET_PCT);
            
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
            
            float heightAboveGround = Mathf.Max(roadWidth * GROUND_SNAP_HEIGHT_PCT, MIN_GROUND_SNAP_HEIGHT);
            return _groundSnapper.SnapToGround(roadPose.Position, heightAboveGround);
        }

        public Vector3 SnapToGround(Vector3 position)
        {
            return _groundSnapper.SnapToGround(position, GROUND_SNAP_HEIGHT);
        }

        private float CalculateDistanceAlongPolyline(
            RoadPathSegment segment,
            RoadSegmentData segmentData,
            float roadWidth)
        {
            float distanceOffset = segmentData.LengthMeters * DISTANCE_ALONG_PCT;
            float clampedDistance = Mathf.Min(distanceOffset, segmentData.LengthMeters * 0.9f);
            return segment.IsForward
                ? clampedDistance
                : segmentData.LengthMeters - clampedDistance;
        }

        private bool TryGetRoadWidth(RoadPathSegment segment, RoadSegmentData segmentData, out float roadWidth)
        {
            roadWidth = 0f;
            if (segment == null || segmentData == null || segmentData.Data == null)
            {
                LogBadSegment(segment, "RoadData отсутствует");
                return false;
            }

            float width = segmentData.Data.LaneWidth * segmentData.Data.LaneCount;
            if (width <= 0f)
            {
                LogBadSegment(segment, "LaneWidth или LaneCount <= 0");
                return false;
            }

            roadWidth = width;
            return true;
        }

        private void LogBadSegment(RoadPathSegment segment, string reason)
        {
            if (segment == null)
            {
                Debug.LogError("ArrowPositionCalculator: сегмент отсутствует.");
                return;
            }

            if (_badSegments.Add(segment.SegmentId))
            {
                Debug.LogError($"ArrowPositionCalculator: не удалось получить ширину дороги для {segment.SegmentId}. Причина: {reason}.");
            }
        }
    }
}

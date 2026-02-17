using System.Collections.Generic;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using Internal.Scripts.UI.Arrow.PositionCalculation;
using UnityEngine;

namespace Internal.Scripts.Road.Positioning
{
    public sealed class RoadSidePositionCalculator : IRoadSidePositionCalculator
    {
        private readonly HashSet<RoadSegmentId> _badSegments = new HashSet<RoadSegmentId>();

        private readonly IRoadNetwork _roadNetwork;
        private readonly RoadSamplerCache _samplerCache;
        private readonly RoadPoseSampler _poseSampler;
        private readonly GroundSnapper _groundSnapper;

        public RoadSidePositionCalculator(
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

        public Vector3 CalculatePosition(
            RoadPathSegment segment,
            RoadLane lane,
            float distanceAlongPct,
            float lateralOffsetPct,
            float heightAboveGround)
        {
            if (!_roadNetwork.TryGetSegment(segment.SegmentId, out RoadSegmentData segmentData))
            {
                Debug.LogError($"[RoadSidePositionCalculator] Segment not found: {segment.SegmentId}");
                return Vector3.zero;
            }
            if (!_samplerCache.TryGetSampler(segmentData.Runtime, out RoadPolylineSampler sampler))
            {
                Debug.LogError($"[RoadSidePositionCalculator] Sampler not found for road");
                return Vector3.zero;
            }

            if (!TryGetRoadWidth(segment, segmentData, out float roadWidth))
                return Vector3.zero;

            float distanceAlongPolyline = CalculateDistanceAlongPolyline(
                segment,
                segmentData,
                distanceAlongPct);
            float lateralOffset = (int)lane * (roadWidth * lateralOffsetPct);

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

            return _groundSnapper.SnapToGround(roadPose.Position, heightAboveGround);
        }

        public Vector3 SnapToGround(Vector3 position, float height)
        {
            return _groundSnapper.SnapToGround(position, height);
        }

        private float CalculateDistanceAlongPolyline(
            RoadPathSegment segment,
            RoadSegmentData segmentData,
            float distanceAlongPct)
        {
            float distanceOffset = segmentData.LengthMeters * distanceAlongPct;
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
                Debug.LogError("RoadSidePositionCalculator: сегмент отсутствует.");
                return;
            }

            if (_badSegments.Add(segment.SegmentId))
            {
                Debug.LogError($"RoadSidePositionCalculator: не удалось получить ширину дороги для {segment.SegmentId}. Причина: {reason}.");
            }
        }
    }
}

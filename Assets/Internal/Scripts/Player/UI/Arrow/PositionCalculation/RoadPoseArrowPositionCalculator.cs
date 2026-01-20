using System;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.Player.UI.Arrow.PositionCalculation
{
    /// <summary>
    /// Вычисляет позицию стрелки на дороге с учетом полосы и бокового смещения
    /// Использует RoadPose для получения точной позиции на полилинии
    /// </summary>
    public sealed class RoadPoseArrowPositionCalculator : IArrowPositionCalculator
    {
        private const float ARROW_HEIGHT = 20f;
        private const float ARROW_DISTANCE_OFFSET = 5f;

        private readonly IRoadNetwork _roadNetwork;
        private readonly RoadSamplerCache _samplerCache;
        private readonly RoadPoseSampler _poseSampler;

        public RoadPoseArrowPositionCalculator(
            IRoadNetwork roadNetwork,
            RoadSamplerCache samplerCache,
            RoadPoseSampler poseSampler)
        {
            _roadNetwork = roadNetwork ?? throw new ArgumentNullException(nameof(roadNetwork));
            _samplerCache = samplerCache ?? throw new ArgumentNullException(nameof(samplerCache));
            _poseSampler = poseSampler ?? throw new ArgumentNullException(nameof(poseSampler));
        }

        public Vector3 CalculateWorldPosition(RoadPathSegment segment, float distanceAlongSegment, RoadLane lane)
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
            
            RoadPose roadPose = _poseSampler.Sample(
                sampler,
                distanceAlongPolyline,
                segmentData.Runtime.WorldRoot,
                segmentData.Runtime.transform,
                segmentData.Data,
                lane,
                0f,
                segment.IsForward
            );

            return new Vector3(roadPose.Position.x, ARROW_HEIGHT, roadPose.Position.z);
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
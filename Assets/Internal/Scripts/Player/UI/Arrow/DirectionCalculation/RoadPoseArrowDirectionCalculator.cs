using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.Player.UI.Arrow.DirectionCalculation
{
    public sealed class RoadPoseArrowDirectionCalculator : IArrowDirectionCalculator
    {
        private const float ARROW_DISTANCE_OFFSET = 5f;

        private readonly IRoadNetwork _roadNetwork;
        private readonly RoadSamplerCache _samplerCache;

        public RoadPoseArrowDirectionCalculator(
            IRoadNetwork roadNetwork,
            RoadSamplerCache samplerCache)
        {
            _roadNetwork = roadNetwork;
            _samplerCache = samplerCache;
        }

        public Vector3 CalculateWorldDirection(RoadPathSegment segment, float distanceAlongSegment)
        {
            if (!_roadNetwork.TryGetSegment(segment.SegmentId, out RoadSegmentData segmentData))
            {
                Debug.LogError($"[ArrowDirectionCalculator] Segment not found: {segment.SegmentId}");
                return Vector3.forward;
            }

            if (!_samplerCache.TryGetSampler(segmentData.Runtime, out RoadPolylineSampler sampler))
            {
                Debug.LogError($"[ArrowDirectionCalculator] Sampler not found for road");
                return Vector3.forward;
            }

            float distanceAlongPolyline = CalculateDistanceAlongPolyline(segment, segmentData);
            Vector3 tangentLocal = sampler.GetTangentLocal(distanceAlongPolyline);
            Vector3 tangentWorld = segmentData.Runtime.LocalDirToWorld(tangentLocal);
            if (segment.IsForward)
            {
                tangentWorld = -tangentWorld;
            }

            return tangentWorld.sqrMagnitude > 0.001f ? tangentWorld.normalized : Vector3.forward;
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
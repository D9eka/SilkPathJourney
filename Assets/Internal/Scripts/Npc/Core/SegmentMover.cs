using System;
using System.Collections.Generic;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.Npc.Core
{
    public class SegmentMover
    {
        public event Action<List<RoadPathSegment>> OnEndSegment;
        
        private readonly IRoadNetwork _network;
        private readonly RoadSamplerCache _samplerCache;
        private readonly RoadPoseSampler _poseSampler;
        
        private RoadPathSegment _currentSegment;
        private RoadLane _lane;
        private float _lateralOffset;
        
        private float _segmentLength;
        private float _distanceOnSegment;
        
        public RoadPose CurrentPose { get; private set; }

        public SegmentMover(IRoadNetwork network, RoadSamplerCache samplerCache, RoadPoseSampler poseSampler)
        {
            _network = network;
            _samplerCache = samplerCache;
            _poseSampler = poseSampler;
        }
        
        public void Initialize(string currentNodeId)
        {
            SetPose(_network.GetOutgoingSegments(currentNodeId)[0]);
        }

        public void SetSegment(RoadPathSegment segment, RoadLane lane, float lateralOffset)
        {
            _currentSegment = segment;
            _lane = lane;
            _lateralOffset = lateralOffset;
            _segmentLength = _currentSegment.LengthMeters;
            _distanceOnSegment = 0f;
            
            UpdatePose();
        }
        
        public float Advance(float deltaMeters)
        {
            if (_currentSegment == null || deltaMeters <= 0f)
                return 0f;

            float moved = 0f;
            float remaining = deltaMeters;

            while (remaining > 0f)
            {
                float leftOnSegment = _segmentLength - _distanceOnSegment;
                float step = Mathf.Min(leftOnSegment, remaining);

                _distanceOnSegment += step;
                moved += step;
                remaining -= step;

                if (_distanceOnSegment >= _segmentLength - Mathf.Epsilon)
                {
                    OnEndSegment?.Invoke(_network.GetOutgoingSegments(_currentSegment.ToNodeId));
                    _currentSegment = null;
                    break;
                }
            }

            UpdatePose();
            return moved;
        }

        public float AdvanceByDaySpeed(float speedMetersPerDay, float dayDelta)
        {
            if (speedMetersPerDay <= 0f || dayDelta <= 0f)
                return 0f;

            float distanceToTravel = speedMetersPerDay * dayDelta * GetCurrentSegmentSpeedMultiplier();
            return Advance(distanceToTravel);
        }
        
        public void SetPose(RoadPathSegment segment)
        {
            CurrentPose = SamplePose(segment, 0f);
        }

        public void Cancel()
        {
            _currentSegment = null;
        }

        private void UpdatePose()
        {
            if (_currentSegment == null) return;

            CurrentPose = SamplePose(_currentSegment, _distanceOnSegment);
        }

        private float GetCurrentSegmentSpeedMultiplier()
        {
            if (_currentSegment == null)
                return 1f;

            return _network.TryGetSegment(_currentSegment.SegmentId, out RoadSegmentData data)
                ? Mathf.Max(0.01f, data.SpeedMultiplier)
                : 1f;
        }

        private RoadPose SamplePose(RoadPathSegment segment, float distanceOnSegment)
        {
            if (!_network.TryGetSegment(segment.SegmentId, out RoadSegmentData data))
                return new RoadPose(Vector3.zero, Vector3.forward);

            if (!_samplerCache.TryGetSampler(data.Runtime, out RoadPolylineSampler sampler))
                return new RoadPose(Vector3.zero, Vector3.forward);

            float clampedDistance = Mathf.Clamp(distanceOnSegment, 0f, data.LengthMeters);
            float distanceAlongPolyline = segment.IsForward
                ? clampedDistance
                : data.LengthMeters - clampedDistance;

            return _poseSampler.Sample(
                sampler,
                distanceAlongPolyline,
                data.Runtime.WorldRoot,
                data.Runtime.transform,
                data.Data,
                _lane,
                _lateralOffset,
                segment.IsForward
            );
        }
    }
}

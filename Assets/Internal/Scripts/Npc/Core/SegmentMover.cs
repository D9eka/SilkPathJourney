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
        public event Action<IEnumerable<RoadPathSegment>> OnEndSegment;
        
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

        public void SetSegment(RoadPathSegment segment, RoadLane lane, float lateralOffset)
        {
            _currentSegment = segment;
            _lane = lane;
            _lateralOffset = lateralOffset;
            _segmentLength = _currentSegment.LengthMeters;
            _distanceOnSegment = 0f;
            
            UpdatePose();
        }
        
        public void Advance(float deltaMeters)
        {
            if (_currentSegment == null || deltaMeters <= 0f)
                return;

            float remaining = deltaMeters;

            while (remaining > 0f)
            {
                float leftOnSegment = _segmentLength - _distanceOnSegment;
                float step = Mathf.Min(leftOnSegment, remaining);

                _distanceOnSegment += step;
                remaining -= step;

                if (_distanceOnSegment >= _segmentLength - Mathf.Epsilon)
                {
                    OnEndSegment?.Invoke(_network.GetOutgoingSegments(_currentSegment.ToNodeId));
                    _currentSegment = null;
                    break;
                }
            }

            UpdatePose();
        }

        private void UpdatePose()
        {
            if (_currentSegment == null) return;

            CurrentPose = SamplePose(_currentSegment, _distanceOnSegment);
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
                isForward: segment.IsForward
            );
        }
    }
}
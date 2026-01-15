using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.Npc.Movement
{
    public sealed class RoadPathCursor
    {
        private readonly IRoadNetwork _network;
        private readonly RoadSamplerCache _samplerCache;
        private readonly RoadPoseSampler _poseSampler;

        private RoadPath _path = RoadPath.Empty;
        private RoadLane _lane;
        private float _lateralOffset;

        private int _segmentIndex;
        private float _distanceOnSegment;
        private RoadPose _currentPose;
        private bool _hasPath;

        public RoadPathCursor(IRoadNetwork network, RoadSamplerCache samplerCache, RoadPoseSampler poseSampler)
        {
            _network = network;
            _samplerCache = samplerCache;
            _poseSampler = poseSampler;
        }

        public bool IsEmpty => !_hasPath;
        public bool IsComplete => !_hasPath || _segmentIndex >= _path.Segments.Count;
        public float TotalLength => _path.TotalLengthMeters;
        public float TravelledDistance => ComputeTravelledDistance();
        public RoadPose CurrentPose => _currentPose;

        public void SetPath(RoadPath path, RoadLane lane, float lateralOffset)
        {
            _path = path ?? RoadPath.Empty;
            _lane = lane;
            _lateralOffset = lateralOffset;
            _segmentIndex = 0;
            _distanceOnSegment = 0f;
            _hasPath = _path.IsValid;
            UpdatePose();
        }

        public void Advance(float deltaMeters)
        {
            if (!_hasPath || deltaMeters <= 0f)
                return;

            float remaining = deltaMeters;

            while (remaining > 0f && _segmentIndex < _path.Segments.Count)
            {
                RoadPathSegment segment = _path.Segments[_segmentIndex];
                float segmentLength = segment.LengthMeters;
                float leftOnSegment = segmentLength - _distanceOnSegment;
                float step = Mathf.Min(leftOnSegment, remaining);

                _distanceOnSegment += step;
                remaining -= step;

                if (_distanceOnSegment >= segmentLength - Mathf.Epsilon)
                {
                    _segmentIndex++;
                    _distanceOnSegment = 0f;
                }
            }

            UpdatePose();
        }

        private float ComputeTravelledDistance()
        {
            if (!_hasPath)
                return 0f;

            float travelled = 0f;
            for (int i = 0; i < _segmentIndex && i < _path.Segments.Count; i++)
                travelled += _path.Segments[i].LengthMeters;

            travelled += _distanceOnSegment;
            return travelled;
        }

        private void UpdatePose()
        {
            if (!_hasPath || _path.Segments.Count == 0)
            {
                _currentPose = new RoadPose(Vector3.zero, Vector3.forward);
                return;
            }

            int segmentIdx = Mathf.Clamp(_segmentIndex, 0, _path.Segments.Count - 1);
            float distanceOnSegment = _distanceOnSegment;

            if (_segmentIndex >= _path.Segments.Count)
                distanceOnSegment = _path.Segments[^1].LengthMeters;

            _currentPose = SamplePose(_path.Segments[segmentIdx], distanceOnSegment);
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

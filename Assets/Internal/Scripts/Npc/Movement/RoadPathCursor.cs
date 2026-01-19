using System;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Path;
using Zenject;

namespace Internal.Scripts.Npc.Movement
{
    public sealed class RoadPathCursor : IInitializable, IDisposable
    {
        private readonly SegmentMover _segmentMover;

        private RoadPath _path = RoadPath.Empty;
        private RoadLane _lane;
        private float _lateralOffset;

        private int _segmentIndex;
        private bool _hasPath;

        public RoadPathCursor(SegmentMover segmentMover)
        {
            _segmentMover = segmentMover;
        }

        public bool IsEmpty => !_hasPath;
        public bool IsComplete => !_hasPath || _segmentIndex >= _path.Segments.Count;
        public RoadPose CurrentPose => _segmentMover.CurrentPose;

        public void Initialize()
        {
            _segmentMover.OnEndSegment += ChooseNextSegment;
        }

        public void Dispose()
        {
            _segmentMover.OnEndSegment -= ChooseNextSegment;
        }
        
        public void SetPath(RoadPath path, RoadLane lane, float lateralOffset)
        {
            _path = path ?? RoadPath.Empty;
            _lane = lane;
            _lateralOffset = lateralOffset;
            _segmentIndex = 0;
            _hasPath = _path.IsValid;
            
            _segmentMover.SetSegment(_path.Segments[_segmentIndex], _lane, _lateralOffset);
        }

        public void Advance(float deltaMeters)
        {
            _segmentMover.Advance(deltaMeters); 
        }
        
        private void ChooseNextSegment()
        {
            _segmentIndex++;
            if (_segmentIndex < _path.Segments.Count)
            {
                _segmentMover.SetSegment(_path.Segments[_segmentIndex], _lane, _lateralOffset);
            }
            else
            {
                _hasPath = false;
            }
        }
    }
}

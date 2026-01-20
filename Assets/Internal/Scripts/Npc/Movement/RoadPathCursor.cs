using System;
using System.Collections.Generic;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.Core.NextSegmentProvider;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Path;
using Zenject;

namespace Internal.Scripts.Npc.Movement
{
    public sealed class RoadPathCursor : IInitializable, IDisposable
    {
        private readonly SegmentMover _segmentMover;
        private readonly INextSegmentProvider _nextSegmentProvider;

        private RoadLane _lane;
        private float _lateralOffset;

        private bool _hasPath;

        public RoadPathCursor(SegmentMover segmentMover, INextSegmentProvider nextSegmentProvider)
        {
            _segmentMover = segmentMover;
            _nextSegmentProvider = nextSegmentProvider;
        }

        public bool IsEmpty => !_hasPath;
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
            _lane = lane;
            _lateralOffset = lateralOffset;
            _hasPath = path.IsValid;
            
            _segmentMover.SetSegment(path.Segments[0], _lane, _lateralOffset);
            if (_nextSegmentProvider is IPathAware pathAware)
            {
                pathAware.SetFullPath(path);
            }
            if (_nextSegmentProvider is ITargetAware targetAware)
            {
                targetAware.SetTargetNodeId(path.Segments[^1].ToNodeId);
            }
        }

        public void Advance(float deltaMeters)
        {
            _segmentMover.Advance(deltaMeters); 
        }
        
        private async void ChooseNextSegment(IEnumerable<RoadPathSegment> options)
        {
            try
            {
                RoadPathSegment nextSegment = await _nextSegmentProvider.ChooseNextAsync(options);
                _segmentMover.SetSegment(nextSegment, _lane, _lateralOffset);
            }
            catch (OperationCanceledException)
            {
                _hasPath = false;
            }
        }
    }
}

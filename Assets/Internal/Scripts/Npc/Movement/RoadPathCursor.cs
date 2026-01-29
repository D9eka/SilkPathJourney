using System;
using System.Collections.Generic;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.NextSegment;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Npc.Movement
{
    public sealed class RoadPathCursor : IDisposable
    {
        private readonly IRoadNetwork _roadNetwork;
        private readonly SegmentMover _segmentMover;
        private readonly INextSegmentProvider _nextSegmentProvider;

        private RoadLane _lane;
        private float _lateralOffset;

        private bool _hasPath;

        public RoadPathCursor(IRoadNetwork roadNetwork, SegmentMover segmentMover, 
            INextSegmentProvider nextSegmentProvider)
        {
            _roadNetwork = roadNetwork;
            _segmentMover = segmentMover;
            _nextSegmentProvider = nextSegmentProvider;
        }

        public bool IsEmpty => !_hasPath;
        public RoadPose CurrentPose => _segmentMover.CurrentPose;

        public void Initialize(string currentNodeId)
        {
            _segmentMover.Initialize(currentNodeId);
            _segmentMover.OnEndSegment += ChooseNextSegment;
        }

        public void Dispose()
        {
            _segmentMover.OnEndSegment -= ChooseNextSegment;
        }
        
        public void SetDestination(string currentNodeId, string destinationNodeId, 
            RoadLane lane, float lateralOffset)
        {
            _lane = lane;
            _lateralOffset = lateralOffset;
            _hasPath = true;
            
            if (_nextSegmentProvider is IDestinationAware targetAware)
            {
                targetAware.SetDestination(destinationNodeId);
            }
            List<RoadPathSegment> ongoings = _roadNetwork.GetOutgoingSegments(currentNodeId);
            _segmentMover.SetPose(ongoings[0]);
            ChooseNextSegment(ongoings);
        }

        public void Advance(float deltaMeters)
        {
            _segmentMover.Advance(deltaMeters); 
        }
        
        private async void ChooseNextSegment(List<RoadPathSegment> options)
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

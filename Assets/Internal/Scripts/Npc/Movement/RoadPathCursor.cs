using System;
using System.Collections.Generic;
using System.Threading;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.NextSegment;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.Npc.Movement
{
    public sealed class RoadPathCursor : IDisposable
    {
        private readonly IRoadNetwork _roadNetwork;
        private readonly SegmentMover _segmentMover;
        private readonly INextSegmentProvider _nextSegmentProvider;
        private readonly IRoadPathFinder _pathFinder;

        private RoadLane _lane;
        private float _lateralOffset;

        private string _destinationNodeId;
        private bool _hasPath;
        private bool _arrived;
        private CancellationTokenSource _chooseCts;

        public RoadPathCursor(IRoadNetwork roadNetwork, SegmentMover segmentMover,
            INextSegmentProvider nextSegmentProvider, IRoadPathFinder pathFinder)
        {
            _roadNetwork = roadNetwork;
            _segmentMover = segmentMover;
            _nextSegmentProvider = nextSegmentProvider;
            _pathFinder = pathFinder;
        }

        public bool IsEmpty => !_hasPath;
        public bool HasArrived => _arrived;
        public RoadPose CurrentPose => _segmentMover.CurrentPose;
        public string CurrentFromNodeId => _segmentMover.CurrentFromNodeId;
        public string CurrentToNodeId => _segmentMover.CurrentToNodeId;
        public float DistanceOnSegment => _segmentMover.DistanceOnSegment;

        public void Initialize(string currentNodeId)
        {
            _segmentMover.Initialize(currentNodeId);
            _segmentMover.OnEndSegment += ChooseNextSegment;
        }

        public void Dispose()
        {
            _segmentMover.OnEndSegment -= ChooseNextSegment;
            _chooseCts?.Cancel();
            _chooseCts?.Dispose();
            _chooseCts = null;
        }
        
        public void UpdateDestination(string destinationNodeId)
        {
            _destinationNodeId = destinationNodeId;
            if (_nextSegmentProvider is IDestinationAware aware)
                aware.SetDestination(destinationNodeId);

            string fromNode = _segmentMover.CurrentFromNodeId;
            string toNode = _segmentMover.CurrentToNodeId;
            if (string.IsNullOrEmpty(fromNode) || string.IsNullOrEmpty(toNode))
                return;

            if (ShouldReverseToReach(fromNode, toNode, destinationNodeId))
                _segmentMover.ReverseCurrentSegment();
        }

        private bool ShouldReverseToReach(string fromNode, string toNode, string target)
        {
            if (target == fromNode) return true;
            if (target == toNode) return false;

            float distToFrom = _segmentMover.DistanceOnSegment;
            float distToTo = _segmentMover.SegmentLength - distToFrom;

            RoadPath viaFrom = _pathFinder.FindPath(fromNode, target);
            RoadPath viaTo = _pathFinder.FindPath(toNode, target);

            bool fromValid = viaFrom != null && viaFrom.IsValid;
            bool toValid = viaTo != null && viaTo.IsValid;

            if (!fromValid && !toValid) return false;
            if (!fromValid) return false;
            if (!toValid) return true;

            float reverseLen = distToFrom + viaFrom.TotalLengthMeters;
            float continueLen = distToTo + viaTo.TotalLengthMeters;
            return reverseLen < continueLen;
        }

        public void SetDestination(string currentNodeId, string destinationNodeId,
            RoadLane lane, float lateralOffset)
        {
            _chooseCts?.Cancel();
            _chooseCts?.Dispose();
            _chooseCts = null;
            _lane = lane;
            _lateralOffset = lateralOffset;
            _destinationNodeId = destinationNodeId;
            _hasPath = true;
            _arrived = false;
            
            if (_nextSegmentProvider is IDestinationAware targetAware)
            {
                targetAware.SetDestination(destinationNodeId);
            }
            List<RoadPathSegment> ongoings = _roadNetwork.GetOutgoingSegments(currentNodeId);
            _segmentMover.SetPose(ongoings[0]);
            ChooseNextSegment(ongoings);
        }

        public void CancelPath()
        {
            _hasPath = false;
            _arrived = false;
            _destinationNodeId = null;
            _chooseCts?.Cancel();
            _chooseCts?.Dispose();
            _chooseCts = null;
            _segmentMover.Cancel();
        }

        public float Advance(float deltaMeters)
        {
            return _segmentMover.Advance(deltaMeters);
        }

        public float AdvanceByDaySpeed(float speedMetersPerDay, float dayDelta)
        {
            return _segmentMover.AdvanceByDaySpeed(speedMetersPerDay, dayDelta);
        }
        
        private async void ChooseNextSegment(List<RoadPathSegment> options)
        {
            try
            {
                _chooseCts?.Cancel();
                _chooseCts?.Dispose();
                _chooseCts = new CancellationTokenSource();

                RoadPathSegment nextSegment = await _nextSegmentProvider.ChooseNextAsync(options, _chooseCts.Token);
                _segmentMover.SetSegment(nextSegment, _lane, _lateralOffset);
            }
            catch (OperationCanceledException)
            {
                _hasPath = false;
                string currentNode = options.Count > 0 ? options[0].FromNodeId : null;
                if (currentNode == _destinationNodeId)
                {
                    _arrived = true;
                }
                else
                {
                    Debug.LogWarning($"[RoadPathCursor] Path failed at '{currentNode}', destination was '{_destinationNodeId}'");
                }
            }
        }
    }
}

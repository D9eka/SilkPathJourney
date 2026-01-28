using System.Collections.Generic;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.Player.Path
{
    public sealed class PathHintsCreator
    {
        private readonly IRoadPathFinder _roadPathFinder;
        private readonly IRoadNetwork _roadNetwork;

        public PathHintsCreator(IRoadPathFinder roadPathFinder, IRoadNetwork roadNetwork)
        {
            _roadPathFinder = roadPathFinder;
            _roadNetwork = roadNetwork;
        }
        
        public PathHints GetPathHints(string fromNodeId, string toNodeId)
        {
            RoadPath fastestPath = _roadPathFinder.FindPath(fromNodeId, toNodeId);
            if (fastestPath.Segments.Count == 0)
            {
                Debug.LogWarning($"[PathHintsCreator] No path found from {fromNodeId} to {toNodeId}");
                return null;
            }

            RoadPathSegment fastestSegment = fastestPath.Segments[0];
            
            List<RoadPathSegment> leadingToTargetSegments = GetReachableOutgoingSegments(fromNodeId, toNodeId, fastestPath);

            Debug.Log($"[PathHints] Fastest: {fastestSegment.SegmentId}, Good segments: {leadingToTargetSegments.Count}");

            return new PathHints(fastestSegment, leadingToTargetSegments);
        }
        
        private List<RoadPathSegment> GetReachableOutgoingSegments(string fromNodeId, string toNodeId, RoadPath fastestPath)
        {
            bool playerIsMovingForward = fastestPath.Segments[0].IsForward;
            
            float fastestPathLength = fastestPath.TotalLengthMeters;
            IEnumerable<RoadPathSegment> outgoingSegments = _roadNetwork.GetOutgoingSegments(fromNodeId);

            List<RoadPathSegment> reachable = new List<RoadPathSegment>();

            foreach (var segment in outgoingSegments)
            {
                if (segment.IsForward != playerIsMovingForward)
                    continue;
                
                RoadPath pathFromSegmentEnd = _roadPathFinder.FindPath(segment.ToNodeId, toNodeId);
                if (pathFromSegmentEnd.Segments.Count == 0)
                    continue;
                if (!IsPathContinuingInRightDirection(segment, pathFromSegmentEnd))
                    continue;

                float pathThroughLength = GetSegmentLength(segment) + pathFromSegmentEnd.TotalLengthMeters;
                if (pathThroughLength <= fastestPathLength * 1.2f)
                {
                    reachable.Add(segment);
                }
            }

            return reachable;
        }
        
        private bool IsPathContinuingInRightDirection(RoadPathSegment currentSegment, RoadPath continuationPath)
        {
            if (continuationPath.Segments.Count == 0)
                return false;
            
            RoadPathSegment firstContinuation = continuationPath.Segments[0];
            return currentSegment.IsForward == firstContinuation.IsForward;
        }
        
        private float GetSegmentLength(RoadPathSegment segment)
        {
            if (_roadNetwork.TryGetSegment(segment.SegmentId, out RoadSegmentData segmentData))
            {
                return segmentData.LengthMeters;
            }

            return 0f;
        }
    }
}
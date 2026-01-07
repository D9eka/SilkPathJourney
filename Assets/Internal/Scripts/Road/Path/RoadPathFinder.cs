using System.Collections.Generic;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Nodes;
using UnityEngine;

namespace Internal.Scripts.Road.Path
{
    public sealed class RoadPathFinder : IRoadPathFinder
    {
        private readonly IRoadNetwork _network;
        private readonly IRoadNodeLookup _nodeLookup;

        public RoadPathFinder(IRoadNetwork network, IRoadNodeLookup nodeLookup)
        {
            _network = network;
            _nodeLookup = nodeLookup;
        }

        public RoadPath FindPath(string startNodeId, string targetNodeId)
        {
            if (string.IsNullOrWhiteSpace(startNodeId) || string.IsNullOrWhiteSpace(targetNodeId))
                return RoadPath.Empty;

            if (startNodeId == targetNodeId)
                return RoadPath.Empty;

            if (!_network.ContainsNode(startNodeId))
            {
                Debug.LogWarning($"[RoadPathFinder] Start node '{startNodeId}' not found.");
                return RoadPath.Empty;
            }

            if (!_network.ContainsNode(targetNodeId))
            {
                Debug.LogWarning($"[RoadPathFinder] Target node '{targetNodeId}' not found.");
                return RoadPath.Empty;
            }

            var dist = new Dictionary<string, float> { [startNodeId] = 0f };
            var prev = new Dictionary<string, RoadGraphEdge>();
            var open = new List<PathNode> { new(startNodeId, 0f) };
            var closed = new HashSet<string>();

            while (open.Count > 0)
            {
                PathNode current = PopMin(open);
                if (!closed.Add(current.NodeId))
                    continue;

                if (current.NodeId == targetNodeId)
                    break;

                foreach (RoadGraphEdge edge in _network.GetOutgoing(current.NodeId))
                {
                    float g = dist[current.NodeId] + edge.Cost;
                    if (dist.TryGetValue(edge.ToNodeId, out float existing) && existing <= g)
                        continue;

                    dist[edge.ToNodeId] = g;
                    prev[edge.ToNodeId] = edge;

                    float h = Heuristic(edge.ToNodeId, targetNodeId);
                    open.Add(new PathNode(edge.ToNodeId, g + h));
                }
            }

            if (!prev.ContainsKey(targetNodeId))
            {
                Debug.LogWarning($"[RoadPathFinder] Path not found {startNodeId} -> {targetNodeId}.");
                return RoadPath.Empty;
            }

            List<RoadGraphEdge> edgePath = Reconstruct(prev, startNodeId, targetNodeId);
            return BuildRoadPath(edgePath);
        }

        private RoadPath BuildRoadPath(List<RoadGraphEdge> edgePath)
        {
            var segments = new List<RoadPathSegment>(edgePath.Count);
            float totalLength = 0f;

            foreach (RoadGraphEdge edge in edgePath)
            {
                if (!_network.TryGetSegment(edge.SegmentId, out RoadSegmentData segmentData))
                    continue;

                totalLength += segmentData.LengthMeters;
                segments.Add(new RoadPathSegment(edge.SegmentId, edge.FromNodeId, edge.ToNodeId, segmentData.LengthMeters));
            }

            return segments.Count == 0 ? RoadPath.Empty : new RoadPath(segments, totalLength);
        }

        private static List<RoadGraphEdge> Reconstruct(Dictionary<string, RoadGraphEdge> prev, string start, string target)
        {
            var edges = new List<RoadGraphEdge>();
            string current = target;

            while (current != start && prev.TryGetValue(current, out RoadGraphEdge edge))
            {
                edges.Add(edge);
                current = edge.FromNodeId;
            }

            edges.Reverse();
            return edges;
        }

        private float Heuristic(string from, string to)
        {
            Vector3? a = _nodeLookup.GetPosition(from);
            Vector3? b = _nodeLookup.GetPosition(to);
            if (a.HasValue && b.HasValue)
                return Vector3.Distance(a.Value, b.Value);

            return 0f;
        }

        private static PathNode PopMin(List<PathNode> list)
        {
            int minIndex = 0;
            float minCost = list[0].Cost;
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].Cost < minCost)
                {
                    minCost = list[i].Cost;
                    minIndex = i;
                }
            }

            PathNode node = list[minIndex];
            list.RemoveAt(minIndex);
            return node;
        }

        private readonly struct PathNode
        {
            public string NodeId { get; }
            public float Cost { get; }

            public PathNode(string nodeId, float cost)
            {
                NodeId = nodeId;
                Cost = cost;
            }
        }
    }
}

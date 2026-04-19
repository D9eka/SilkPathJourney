using System.Collections.Generic;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Npc.Editor.Headless
{
    public sealed class HeadlessRoadNetwork : IRoadNetwork
    {
        private static readonly List<RoadGraphEdge> EmptyEdges = new();
        private static readonly List<RoadPathSegment> EmptySegments = new();

        private readonly HashSet<string> _nodes;
        private readonly Dictionary<string, List<RoadGraphEdge>> _outgoingEdges;
        private readonly Dictionary<string, List<RoadPathSegment>> _outgoingSegments;
        private readonly Dictionary<RoadSegmentId, RoadSegmentData> _segments;

        public IEnumerable<string> Nodes => _nodes;
        public IReadOnlyDictionary<RoadSegmentId, RoadSegmentData> Segments => _segments;

        public HeadlessRoadNetwork(RoadGraphSnapshot snapshot)
        {
            _nodes = new HashSet<string>(snapshot.Nodes.Count);
            foreach (var node in snapshot.Nodes)
                _nodes.Add(node.Id);

            _outgoingEdges = new Dictionary<string, List<RoadGraphEdge>>();
            _outgoingSegments = new Dictionary<string, List<RoadPathSegment>>();
            _segments = new Dictionary<RoadSegmentId, RoadSegmentData>();

            foreach (var edge in snapshot.Edges)
            {
                var segId = new RoadSegmentId(edge.RoadId, edge.Forward);
                var graphEdge = new RoadGraphEdge(edge.FromId, edge.ToId, segId, edge.LengthMeters, edge.Cost);
                var pathSegment = new RoadPathSegment(segId, edge.FromId, edge.ToId, edge.LengthMeters);

                if (!_outgoingEdges.TryGetValue(edge.FromId, out var edgeList))
                {
                    edgeList = new List<RoadGraphEdge>();
                    _outgoingEdges[edge.FromId] = edgeList;
                }
                edgeList.Add(graphEdge);

                if (!_outgoingSegments.TryGetValue(edge.FromId, out var segList))
                {
                    segList = new List<RoadPathSegment>();
                    _outgoingSegments[edge.FromId] = segList;
                }
                segList.Add(pathSegment);

                if (!_segments.ContainsKey(segId))
                {
                    _segments[segId] = new RoadSegmentData(
                        segId,
                        runtime: null,
                        data: null,
                        edge.LengthMeters,
                        edge.Bidirectional,
                        edge.SpeedMul);
                }
            }
        }

        public bool ContainsNode(string nodeId) => _nodes.Contains(nodeId);

        public List<RoadGraphEdge> GetOutgoingEdges(string nodeId)
        {
            _outgoingEdges.TryGetValue(nodeId, out var list);
            return list ?? EmptyEdges;
        }

        public List<RoadPathSegment> GetOutgoingSegments(string nodeId)
        {
            _outgoingSegments.TryGetValue(nodeId, out var list);
            return list ?? EmptySegments;
        }

        public bool TryGetSegment(RoadSegmentId id, out RoadSegmentData data) =>
            _segments.TryGetValue(id, out data);

        public bool TryGetSegment(string fromNode, string toNode, out RoadPathSegment segment)
        {
            segment = null;
            if (string.IsNullOrEmpty(fromNode))
                return false;

            foreach (RoadPathSegment seg in GetOutgoingSegments(fromNode))
            {
                if (seg.ToNodeId == toNode)
                {
                    segment = seg;
                    return true;
                }
            }
            return false;
        }
    }
}

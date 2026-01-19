using System.Collections.Generic;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Road.Graph
{
    public interface IRoadNetwork
    {
        bool ContainsNode(string nodeId);
        IEnumerable<string> Nodes { get; }
        IEnumerable<RoadGraphEdge> GetOutgoingEdges(string nodeId);
        IEnumerable<RoadPathSegment> GetOutgoingSegments(string nodeId);
        bool TryGetSegment(RoadSegmentId id, out RoadSegmentData data);
        IReadOnlyDictionary<RoadSegmentId, RoadSegmentData> Segments { get; }
    }
}

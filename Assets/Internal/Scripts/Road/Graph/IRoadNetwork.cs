using System.Collections.Generic;

namespace Internal.Scripts.Road.Graph
{
    public interface IRoadNetwork
    {
        bool ContainsNode(string nodeId);
        IEnumerable<string> Nodes { get; }
        IEnumerable<RoadGraphEdge> GetOutgoing(string nodeId);
        bool TryGetSegment(RoadSegmentId id, out RoadSegmentData data);
        IReadOnlyDictionary<RoadSegmentId, RoadSegmentData> Segments { get; }
    }
}

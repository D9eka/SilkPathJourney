namespace Internal.Scripts.Road.Graph
{
    public sealed class RoadGraphEdge
    {
        public string FromNodeId { get; }
        public string ToNodeId { get; }
        public RoadSegmentId SegmentId { get; }
        public float Cost { get; }
        public float LengthMeters { get; }

        public RoadGraphEdge(string fromNodeId, string toNodeId, RoadSegmentId segmentId, float lengthMeters, float cost)
        {
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
            SegmentId = segmentId;
            LengthMeters = lengthMeters;
            Cost = cost;
        }
    }
}

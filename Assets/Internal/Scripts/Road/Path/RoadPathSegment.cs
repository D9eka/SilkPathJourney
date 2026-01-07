using Internal.Scripts.Road.Graph;

namespace Internal.Scripts.Road.Path
{
    public sealed class RoadPathSegment
    {
        public RoadSegmentId SegmentId { get; }
        public string FromNodeId { get; }
        public string ToNodeId { get; }
        public float LengthMeters { get; }
        public bool IsForward => SegmentId.Forward;

        public RoadPathSegment(RoadSegmentId segmentId, string fromNodeId, string toNodeId, float lengthMeters)
        {
            SegmentId = segmentId;
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
            LengthMeters = lengthMeters;
        }
    }
}

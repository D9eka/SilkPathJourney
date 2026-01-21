using System;
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

        public override bool Equals(object obj)
        {
            return obj is RoadPathSegment other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(SegmentId, FromNodeId, ToNodeId, LengthMeters);
        }

        private bool Equals(RoadPathSegment other)
        {
            return FromNodeId == other.FromNodeId && 
                ToNodeId == other.ToNodeId && 
                LengthMeters.Equals(other.LengthMeters);
        }
    }
}

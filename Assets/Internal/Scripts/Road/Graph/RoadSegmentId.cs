using System;

namespace Internal.Scripts.Road.Graph
{
    public readonly struct RoadSegmentId : IEquatable<RoadSegmentId>
    {
        public string RoadId { get; }
        public bool Forward { get; }

        public RoadSegmentId(string roadId, bool forward)
        {
            RoadId = roadId;
            Forward = forward;
        }

        public bool Equals(RoadSegmentId other) => RoadId == other.RoadId && Forward == other.Forward;

        public override bool Equals(object obj) => obj is RoadSegmentId other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(RoadId, Forward);

        public static bool operator ==(RoadSegmentId x, RoadSegmentId y) => Equals(x, y);
        public static bool operator !=(RoadSegmentId x, RoadSegmentId y) => !Equals(x, y);

        public override string ToString() => $"{RoadId}:{(Forward ? "F" : "B")}";
    }
}

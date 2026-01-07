using System.Collections.Generic;

namespace Internal.Scripts.Road.Path
{
    public sealed class RoadPath
    {
        public static readonly RoadPath Empty = new(new List<RoadPathSegment>(), 0f);

        public IReadOnlyList<RoadPathSegment> Segments { get; }
        public float TotalLengthMeters { get; }
        public bool IsValid => Segments.Count > 0;

        public RoadPath(IReadOnlyList<RoadPathSegment> segments, float totalLengthMeters)
        {
            Segments = segments;
            TotalLengthMeters = totalLengthMeters;
        }
    }
}

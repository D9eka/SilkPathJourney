using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Road.Path
{
    public sealed class RoadPath
    {
        public static readonly RoadPath Empty = new RoadPath(new List<RoadPathSegment>(), 0f);

        public IReadOnlyList<RoadPathSegment> Segments { get; }
        public float TotalLengthMeters { get; }
        public bool IsValid => Segments.Count > 0;

        public int EstimateDays(float speedMetersPerDay) =>
            speedMetersPerDay > 0f ? Mathf.CeilToInt(TotalLengthMeters / speedMetersPerDay) : -1;

        public RoadPath(IReadOnlyList<RoadPathSegment> segments, float totalLengthMeters)
        {
            Segments = segments;
            TotalLengthMeters = totalLengthMeters;
        }
    }
}

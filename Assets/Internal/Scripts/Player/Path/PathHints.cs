using System.Collections.Generic;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Player.Path
{
    public class PathHints
    {
        public RoadPathSegment FastestSegment { get; private set; }
        public List<RoadPathSegment> LeadingToTargetSegments { get; private set; }

        public PathHints(RoadPathSegment fastestSegment, List<RoadPathSegment> leadingToTargetSegments)
        {
            FastestSegment = fastestSegment;
            LeadingToTargetSegments = leadingToTargetSegments;
        }
    }
}
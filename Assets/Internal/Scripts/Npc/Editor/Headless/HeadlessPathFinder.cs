using System.Collections.Generic;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Npc.Editor.Headless
{
    public sealed class HeadlessPathFinder : IRoadPathFinder
    {
        private readonly RoadGraphSnapshot _snapshot;

        public HeadlessPathFinder(RoadGraphSnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        public RoadPath FindPath(string startNodeId, string targetNodeId)
        {
            float distance = _snapshot.GetDistance(startNodeId, targetNodeId);
            if (distance >= float.MaxValue)
                return RoadPath.Empty;

            var dummySegId = new RoadSegmentId($"{startNodeId}>{targetNodeId}", true);
            var dummySeg = new RoadPathSegment(dummySegId, startNodeId, targetNodeId, distance);
            return new RoadPath(new List<RoadPathSegment> { dummySeg }, distance);
        }
    }
}

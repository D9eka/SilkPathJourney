using System.Collections.Generic;
using Internal.Scripts.Road.Graph;

namespace Internal.Scripts.Road.Pathfinder
{
    public interface IRoadPathfinderStrategy
    {
        public List<RoadNode> FindPathNodes(RoadGraph graph, RoadNode start, RoadNode goal);
    }
}
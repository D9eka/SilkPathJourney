using System.Collections.Generic;
using Internal.Scripts.Road.Graph;

namespace Internal.Scripts.Road.Generator
{
    public class RoadGraphGenerator
    {
        private Dictionary<int, RoadNode> _nodeMap;

        public RoadGraph BuildGraph(NodeAuthoring[] nodes, EdgeAuthoring[] edges)
        {
            RoadGraph graph = new RoadGraph();
            _nodeMap = new Dictionary<int, RoadNode>();

            foreach (NodeAuthoring nodeAuthoring in nodes)
            {
                if (nodeAuthoring._transform == null)
                {
                    continue;
                }
                RoadNode node = graph.CreateNode(nodeAuthoring._id, nodeAuthoring._transform.position);
                _nodeMap.Add(nodeAuthoring._id, node);
            }

            foreach (EdgeAuthoring e in edges)
            {
                if (!_nodeMap.TryGetValue(e._prevNodeId, out RoadNode prev)) prev = _nodeMap[e._fromNodeId];
                if (!_nodeMap.TryGetValue(e._nextNodeId, out RoadNode next)) next = _nodeMap[e._toNodeId];

                RoadNode from = _nodeMap[e._fromNodeId];
                RoadNode to = _nodeMap[e._toNodeId];

                graph.ConnectCatmullRom(prev, from, to, next);
            }
            return graph;
        }
    }
}

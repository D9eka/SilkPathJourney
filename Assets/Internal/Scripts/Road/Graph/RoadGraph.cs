using System.Collections.Generic;
using Internal.Scripts.Road.Paths;
using UnityEngine;

namespace Internal.Scripts.Road.Graph
{
    public sealed class RoadGraph
    {
        private readonly Dictionary<int, RoadNode> _nodes = new Dictionary<int, RoadNode>();
        private readonly List<RoadEdge> _edges = new List<RoadEdge>();

        public IReadOnlyDictionary<int, RoadNode> Nodes => _nodes;
        public IReadOnlyList<RoadEdge> Edges => _edges;

        public RoadNode CreateNode(int id, Vector3 position)
        {
            RoadNode node = new RoadNode(id, position);
            _nodes.Add(id, node);
            return node;
        }

        public bool TryGetNode(int id, out RoadNode node) => _nodes.TryGetValue(id, out node);

        /// <summary>
        /// Создаёт ребро с Catmull-Rom сплайном.
        /// prev и next используются для плавности, могут совпадать с from/to.
        /// </summary>
        public RoadEdge ConnectCatmullRom(RoadNode prev, RoadNode from, RoadNode to, RoadNode next, int samples = 32)
        {
            CatmullRomPath path = new CatmullRomPath(prev.Position, from.Position, to.Position, next.Position, samples);
            RoadEdge edge = new RoadEdge(from, to, path);
            _edges.Add(edge);
            from.AddOutgoing(edge);
            return edge;
        }
    }
}
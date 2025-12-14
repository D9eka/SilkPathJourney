using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Road.Graph
{
    public sealed class RoadNode
    {
        public int Id { get; }
        public Vector3 Position { get; }
        private readonly List<RoadEdge> _outgoing = new List<RoadEdge>();

        public IReadOnlyList<RoadEdge> OutgoingEdges => _outgoing;

        public RoadNode(int id, Vector3 position)
        {
            Id = id;
            Position = position;
        }

        internal void AddOutgoing(RoadEdge edge)
        {
            _outgoing.Add(edge);
        }

        public override string ToString() => $"Node {Id} @ {Position}";
    }
}
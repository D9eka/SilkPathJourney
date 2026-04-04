using System.Collections.Generic;
using Internal.Scripts.Road.Nodes;
using UnityEngine;

namespace Internal.Scripts.Npc.Editor.Headless
{
    public sealed class HeadlessRoadNodeLookup : IRoadNodeLookup
    {
        private readonly RoadGraphSnapshot _snapshot;
        private readonly IReadOnlyDictionary<string, Transform> _nodes;

        public HeadlessRoadNodeLookup(RoadGraphSnapshot snapshot)
        {
            _snapshot = snapshot;

            var dict = new Dictionary<string, Transform>(snapshot.Nodes.Count);
            foreach (var node in snapshot.Nodes)
                dict[node.Id] = null;
            _nodes = dict;
        }

        public IReadOnlyDictionary<string, Transform> Nodes => _nodes;

        public bool TryGetTransform(string nodeId, out Transform transform)
        {
            transform = null;
            return _nodes.ContainsKey(nodeId);
        }

        public Vector3? GetPosition(string nodeId)
        {
            if (!_nodes.ContainsKey(nodeId))
                return null;
            return _snapshot.GetPosition(nodeId);
        }

        public bool Contains(string nodeId) => _nodes.ContainsKey(nodeId);

        public string FindNearestNodeId(Vector3 position)
        {
            string nearest = null;
            float bestSqr = float.MaxValue;
            foreach (var node in _snapshot.Nodes)
            {
                float sqr = (node.Position - position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = node.Id;
                }
            }
            return nearest;
        }

        public string FindNearestAmong(string fromNodeId, List<string> candidates)
        {
            Vector3 origin = _snapshot.GetPosition(fromNodeId);
            string nearest = null;
            float bestSqr = float.MaxValue;
            foreach (string candidateId in candidates)
            {
                Vector3 pos = _snapshot.GetPosition(candidateId);
                float sqr = (pos - origin).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = candidateId;
                }
            }
            return nearest;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Road.Nodes
{
    public sealed class RoadNodeLookup : IInitializable, IRoadNodeLookup
    {
        private readonly Dictionary<string, Transform> _nodes = new();

        public IReadOnlyDictionary<string, Transform> Nodes => _nodes;

        public void Initialize()
        {
            _nodes.Clear();

            Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);

            foreach (Transform t in transforms.Where(t => t != null && t.name.StartsWith(NodeIdRules.NodePrefix)))
            {
                string id = t.name;
                if (_nodes.ContainsKey(id))
                {
                    Debug.LogWarning($"[RoadNodeLookup] Duplicate node id '{id}' found on '{t.name}'. Using the first occurrence.");
                    continue;
                }

                _nodes[id] = t;
            }

            if (_nodes.Count == 0)
                Debug.LogWarning($"[RoadNodeLookup] No road nodes found with prefix '{NodeIdRules.NodePrefix}'. Pathfinding will fail.");
        }

        public bool TryGetTransform(string nodeId, out Transform transform) => _nodes.TryGetValue(nodeId, out transform);

        public Vector3? GetPosition(string nodeId)
        {
            if (_nodes.TryGetValue(nodeId, out Transform t) && t != null)
                return t.position;

            return null;
        }

        public bool Contains(string nodeId) => _nodes.ContainsKey(nodeId);

        public string FindNearestNodeId(Vector3 position)
        {
            string nearest = null;
            float minDist = float.MaxValue;

            foreach (var kvp in _nodes)
            {
                if (kvp.Value == null) continue;
                float dist = Vector3.Distance(position, kvp.Value.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = kvp.Key;
                }
            }

            return nearest;
        }

        public string FindNearestAmong(string fromNodeId, List<string> candidates)
        {
            if (candidates.Count == 0)
                return fromNodeId;

            if (!TryGetTransform(fromNodeId, out Transform fromTransform))
                return candidates[0];

            Vector3 fromPos = fromTransform.position;
            string closest = candidates[0];
            float closestDist = float.MaxValue;

            foreach (string nodeId in candidates)
            {
                if (!TryGetTransform(nodeId, out Transform t))
                    continue;

                float dist = (t.position - fromPos).sqrMagnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = nodeId;
                }
            }

            return closest;
        }
    }
}

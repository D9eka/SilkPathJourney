using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Road.Nodes
{
    public interface IRoadNodeLookup
    {
        IReadOnlyDictionary<string, Transform> Nodes { get; }
        bool TryGetTransform(string nodeId, out Transform transform);
        Vector3? GetPosition(string nodeId);
        bool Contains(string nodeId);
        string FindNearestNodeId(Vector3 position);
        string FindNearestAmong(string fromNodeId, List<string> candidates);
    }
}

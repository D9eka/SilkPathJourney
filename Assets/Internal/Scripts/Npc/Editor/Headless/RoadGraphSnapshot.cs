using System;
using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Npc.Editor.Headless
{
    public sealed class RoadGraphSnapshot : ScriptableObject
    {
        [Serializable] public struct NodeEntry { public string Id; public Vector3 Position; }
        [Serializable] public struct EdgeEntry { public string FromId; public string ToId; public string RoadId; public bool Forward; public float LengthMeters; public float SpeedMul; public float Cost; public bool Bidirectional; }
        [Serializable] public struct CityNodeEntry { public string NodeId; public string CityId; }

        [SerializeField] private List<NodeEntry> _nodes = new();
        [SerializeField] private List<EdgeEntry> _edges = new();
        [SerializeField] private List<CityNodeEntry> _cityNodes = new();
        [SerializeField] private float[] _distanceMatrix = Array.Empty<float>();
        [SerializeField] private string[] _nodeIndex = Array.Empty<string>();

        public IReadOnlyList<NodeEntry> Nodes => _nodes;
        public IReadOnlyList<EdgeEntry> Edges => _edges;
        public IReadOnlyList<CityNodeEntry> CityNodes => _cityNodes;
        public IReadOnlyList<string> NodeIndex => _nodeIndex;

        private Dictionary<string, int> _nodeIndexMap;
        private Dictionary<string, Vector3> _positionMap;

        private void BuildIndexMap()
        {
            if (_nodeIndexMap != null) return;
            _nodeIndexMap = new Dictionary<string, int>(_nodeIndex.Length);
            for (int i = 0; i < _nodeIndex.Length; i++)
                _nodeIndexMap[_nodeIndex[i]] = i;
        }

        private void BuildPositionMap()
        {
            if (_positionMap != null) return;
            _positionMap = new Dictionary<string, Vector3>(_nodes.Count);
            foreach (var node in _nodes)
                _positionMap[node.Id] = node.Position;
        }

        public float GetDistance(string fromNodeId, string toNodeId)
        {
            BuildIndexMap();
            if (!_nodeIndexMap.TryGetValue(fromNodeId, out int fromIdx) ||
                !_nodeIndexMap.TryGetValue(toNodeId, out int toIdx))
                return float.MaxValue;
            return _distanceMatrix[fromIdx * _nodeIndex.Length + toIdx];
        }

        public Vector3 GetPosition(string nodeId)
        {
            BuildPositionMap();
            return _positionMap.TryGetValue(nodeId, out Vector3 pos) ? pos : Vector3.zero;
        }

        public void SetData(List<NodeEntry> nodes, List<EdgeEntry> edges, List<CityNodeEntry> cityNodes, float[] distanceMatrix, string[] nodeIndex)
        {
            _nodes = nodes;
            _edges = edges;
            _cityNodes = cityNodes;
            _distanceMatrix = distanceMatrix;
            _nodeIndex = nodeIndex;
            _nodeIndexMap = null;
            _positionMap = null;
        }
    }
}

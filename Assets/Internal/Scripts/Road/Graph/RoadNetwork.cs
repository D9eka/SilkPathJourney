using System;
using System.Collections.Generic;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Road.Path;
using Internal.Scripts.Road.State;
using Internal.Scripts.World.Roads;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Road.Graph
{
    public sealed class RoadNetwork : IRoadNetwork, IInitializable
    {
        private readonly RoadRuntime[] _roadRuntimes;
        private readonly IRoadNodeLookup _nodeLookup;
        private readonly RoadSamplerCache _samplerCache;
        private readonly RoadUnlockService _unlockService;

        private readonly HashSet<string> _nodes = new();
        private readonly Dictionary<string, List<RoadGraphEdge>> _edges = new();
        private readonly Dictionary<RoadSegmentId, RoadSegmentData> _segments = new();
        private readonly Dictionary<string, RoadRuntime> _runtimeByRoadId = new();

        public IReadOnlyDictionary<RoadSegmentId, RoadSegmentData> Segments => _segments;
        public IEnumerable<string> Nodes => _nodes;

        public RoadNetwork(RoadRuntime[] roadRuntimes, IRoadNodeLookup nodeLookup, RoadSamplerCache samplerCache, RoadUnlockService unlockService)
        {
            _roadRuntimes = roadRuntimes ?? Array.Empty<RoadRuntime>();
            _nodeLookup = nodeLookup;
            _samplerCache = samplerCache;
            _unlockService = unlockService;
        }

        public void Initialize()
        {
            _nodes.Clear();
            _edges.Clear();
            _segments.Clear();
            _runtimeByRoadId.Clear();

            foreach (RoadRuntime runtime in _roadRuntimes)
            {
                if (runtime == null || runtime.Data == null)
                    continue;

                RoadData data = runtime.Data;
                if (string.IsNullOrWhiteSpace(data.RoadId) ||
                    string.IsNullOrWhiteSpace(data.StartNodeId) ||
                    string.IsNullOrWhiteSpace(data.EndNodeId))
                {
                    Debug.LogWarning($"[RoadNetwork] RoadRuntime '{runtime.name}' has invalid RoadData (missing ids). Skipping.");
                    continue;
                }

                _runtimeByRoadId[data.RoadId] = runtime;

                if (data.IsHidden && !_unlockService.IsUnlocked(data.RoadId))
                {
                    runtime.gameObject.SetActive(false);
                    continue;
                }

                if (!_nodeLookup.Contains(data.StartNodeId) || !_nodeLookup.Contains(data.EndNodeId))
                {
                    Debug.LogWarning($"[RoadNetwork] Nodes for road '{data.RoadId}' not found ({data.StartNodeId}->{data.EndNodeId}). Skipping.");
                    continue;
                }

                if (!_samplerCache.TryGetSampler(runtime, out RoadPolylineSampler sampler))
                {
                    Debug.LogWarning($"[RoadNetwork] Failed to build sampler for road '{data.RoadId}'. Skipping.");
                    continue;
                }

                float length = sampler.Length;
                float speedMul = Mathf.Max(0.01f, data.SpeedMul);
                float cost = length / speedMul;

                AddEdge(data.StartNodeId, data.EndNodeId, new RoadSegmentId(data.RoadId, true), runtime, data, length, speedMul, cost);

                if (data.Bidirectional)
                {
                    AddEdge(data.EndNodeId, data.StartNodeId, new RoadSegmentId(data.RoadId, false), runtime, data, length, speedMul, cost);
                }
            }

            if (_edges.Count == 0)
                Debug.LogWarning("[RoadNetwork] No valid road edges were built. Pathfinding will fail.");
        }

        public void UnlockRoad(string roadId)
        {
            if (!_runtimeByRoadId.TryGetValue(roadId, out RoadRuntime runtime))
            {
                Debug.LogWarning($"[RoadNetwork] Cannot unlock road '{roadId}': runtime not found.");
                return;
            }

            RoadData data = runtime.Data;
            runtime.gameObject.SetActive(true);

            if (!_samplerCache.TryGetSampler(runtime, out RoadPolylineSampler sampler))
            {
                Debug.LogWarning($"[RoadNetwork] Failed to build sampler for unlocked road '{roadId}'.");
                return;
            }

            float length = sampler.Length;
            float speedMul = Mathf.Max(0.01f, data.SpeedMul);
            float cost = length / speedMul;

            AddEdge(data.StartNodeId, data.EndNodeId, new RoadSegmentId(data.RoadId, true), runtime, data, length, speedMul, cost);

            if (data.Bidirectional)
            {
                AddEdge(data.EndNodeId, data.StartNodeId, new RoadSegmentId(data.RoadId, false), runtime, data, length, speedMul, cost);
            }
        }

        public bool ContainsNode(string nodeId) => _nodes.Contains(nodeId);

        public List<RoadGraphEdge> GetOutgoingEdges(string nodeId)
        {
            return _edges.TryGetValue(nodeId, out List<RoadGraphEdge> list)
                ? list
                : new List<RoadGraphEdge>();
        }

        public List<RoadPathSegment> GetOutgoingSegments(string nodeId)
        {
            List<RoadPathSegment> ongoings = new List<RoadPathSegment>();
            IEnumerable<RoadGraphEdge> ongoingsEdges = GetOutgoingEdges(nodeId);
            foreach (RoadGraphEdge ongoing in ongoingsEdges)
            {
                if (TryGetSegment(ongoing.SegmentId, out RoadSegmentData segmentData))
                {
                    ongoings.Add(new RoadPathSegment(ongoing.SegmentId, ongoing.FromNodeId, ongoing.ToNodeId, segmentData.LengthMeters));
                }
            }
            return ongoings;
        }

        public bool TryGetSegment(RoadSegmentId id, out RoadSegmentData data) => _segments.TryGetValue(id, out data);

        private void AddEdge(string from, string to, RoadSegmentId id, RoadRuntime runtime, RoadData data, float length, float speedMul, float cost)
        {
            var edge = new RoadGraphEdge(from, to, id, length, cost);

            if (!_edges.TryGetValue(from, out List<RoadGraphEdge> list))
            {
                list = new List<RoadGraphEdge>();
                _edges[from] = list;
            }

            list.Add(edge);
            _nodes.Add(from);
            _nodes.Add(to);
            _segments[id] = new RoadSegmentData(id, runtime, data, length, data.Bidirectional, speedMul);
        }
    }
}

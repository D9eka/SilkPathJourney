using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Road.Path;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Npc.Editor.Headless
{
    public static class RoadGraphSnapshotBuilder
    {
        private const string AssetPath = "Assets/Internal/Data/RoadGraphSnapshot.asset";

        [MenuItem("SPJ/Debug/Capture Road Graph Snapshot")]
        public static void Capture()
        {
            // 1. Collect nodes — GameObjects whose name starts with "N_"
            var nodes = new List<RoadGraphSnapshot.NodeEntry>();
            var nodePositions = new Dictionary<string, Vector3>();

            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (!t.name.StartsWith(NodeIdRules.NodePrefix)) continue;
                string nodeId = t.name;
                var entry = new RoadGraphSnapshot.NodeEntry { Id = nodeId, Position = t.position };
                nodes.Add(entry);
                nodePositions[nodeId] = t.position;
            }

            // 2. Collect edges from RoadRuntime components
            var edges = new List<RoadGraphSnapshot.EdgeEntry>();
            var adjacency = new Dictionary<string, List<(string toId, float cost)>>();

            foreach (var runtime in Object.FindObjectsByType<RoadRuntime>(FindObjectsSortMode.None))
            {
                var data = runtime.Data;
                if (data == null || data.IsHidden) continue;
                if (string.IsNullOrEmpty(data.StartNodeId) || string.IsNullOrEmpty(data.EndNodeId)) continue;
                if (!nodePositions.ContainsKey(data.StartNodeId) || !nodePositions.ContainsKey(data.EndNodeId)) continue;

                float length;
                if (data.PointsLocal != null && data.PointsLocal.Count >= 2)
                {
                    // Sum world-space segment lengths, accounting for WorldRoot transform
                    float sum = 0f;
                    for (int i = 1; i < data.PointsLocal.Count; i++)
                    {
                        Vector3 a = runtime.LocalToWorld(data.PointsLocal[i - 1]);
                        Vector3 b = runtime.LocalToWorld(data.PointsLocal[i]);
                        sum += Vector3.Distance(a, b);
                    }
                    length = sum;
                }
                else
                {
                    length = Vector3.Distance(nodePositions[data.StartNodeId], nodePositions[data.EndNodeId]);
                }

                float speedMul = Mathf.Max(0.01f, data.SpeedMul);
                float cost = length / speedMul;

                edges.Add(new RoadGraphSnapshot.EdgeEntry
                {
                    FromId = data.StartNodeId, ToId = data.EndNodeId,
                    RoadId = data.RoadId, Forward = true,
                    LengthMeters = length, SpeedMul = speedMul,
                    Cost = cost, Bidirectional = data.Bidirectional
                });
                DijkstraDistanceCalculator.AddAdj(adjacency, data.StartNodeId, data.EndNodeId, cost);

                if (data.Bidirectional)
                {
                    edges.Add(new RoadGraphSnapshot.EdgeEntry
                    {
                        FromId = data.EndNodeId, ToId = data.StartNodeId,
                        RoadId = data.RoadId, Forward = false,
                        LengthMeters = length, SpeedMul = speedMul,
                        Cost = cost, Bidirectional = true
                    });
                    DijkstraDistanceCalculator.AddAdj(adjacency, data.EndNodeId, data.StartNodeId, cost);
                }
            }

            // 3. Collect city-node links
            var cityNodes = new List<RoadGraphSnapshot.CityNodeEntry>();
            foreach (var link in Object.FindObjectsByType<CityNodeLink>(FindObjectsSortMode.None))
            {
                if (link.City == null) continue;
                cityNodes.Add(new RoadGraphSnapshot.CityNodeEntry
                {
                    NodeId = link.gameObject.name,
                    CityId = link.CityId
                });
            }

            // 4. All-pairs shortest path (Dijkstra per source)
            string[] nodeIndex = nodes.Select(n => n.Id).ToArray();
            int n = nodeIndex.Length;
            float[] matrix = new float[n * n];

            var indexMap = new Dictionary<string, int>(n);
            for (int i = 0; i < n; i++)
                indexMap[nodeIndex[i]] = i;

            for (int i = 0; i < matrix.Length; i++)
                matrix[i] = float.MaxValue;
            for (int i = 0; i < n; i++)
                matrix[i * n + i] = 0f;

            for (int src = 0; src < n; src++)
            {
                DijkstraDistanceCalculator.RunDijkstra(src, nodeIndex, indexMap, adjacency, matrix, n);

                if (src % 10 == 0)
                    EditorUtility.DisplayProgressBar("Building Distance Matrix", $"Dijkstra {src}/{n}", (float)src / n);
            }
            EditorUtility.ClearProgressBar();

            // 5. Save / update asset
            var snapshot = AssetDatabase.LoadAssetAtPath<RoadGraphSnapshot>(AssetPath);
            if (snapshot == null)
            {
                snapshot = ScriptableObject.CreateInstance<RoadGraphSnapshot>();
                AssetDatabase.CreateAsset(snapshot, AssetPath);
            }
            snapshot.SetData(nodes, edges, cityNodes, matrix, nodeIndex);
            EditorUtility.SetDirty(snapshot);
            AssetDatabase.SaveAssets();

            Debug.Log($"[RoadGraphSnapshotBuilder] Captured: {nodes.Count} nodes, {edges.Count} edges, {cityNodes.Count} city links. Matrix: {n}x{n}");
        }
    }
}

using System.Collections.Generic;

namespace Internal.Scripts.Npc.Editor.Headless
{
    public static class DijkstraDistanceCalculator
    {
        public static void RunDijkstra(int srcIdx, string[] nodeIndex, Dictionary<string, int> indexMap,
            Dictionary<string, List<(string toId, float cost)>> adjacency, float[] matrix, int n)
        {
            var dist = new float[n];
            for (int i = 0; i < n; i++) dist[i] = float.MaxValue;
            dist[srcIdx] = 0f;

            var visited = new bool[n];

            for (int step = 0; step < n; step++)
            {
                int u = -1;
                float minDist = float.MaxValue;
                for (int i = 0; i < n; i++)
                {
                    if (!visited[i] && dist[i] < minDist)
                    {
                        minDist = dist[i];
                        u = i;
                    }
                }
                if (u < 0) break;
                visited[u] = true;

                if (!adjacency.TryGetValue(nodeIndex[u], out var neighbors)) continue;

                foreach (var (toId, cost) in neighbors)
                {
                    if (!indexMap.TryGetValue(toId, out int v) || visited[v]) continue;
                    float alt = dist[u] + cost;
                    if (alt < dist[v]) dist[v] = alt;
                }
            }

            for (int i = 0; i < n; i++)
                matrix[srcIdx * n + i] = dist[i];
        }

        public static void AddAdj(Dictionary<string, List<(string toId, float cost)>> adj, string from, string to, float cost)
        {
            if (!adj.TryGetValue(from, out var list))
            {
                list = new List<(string, float)>();
                adj[from] = list;
            }
            list.Add((to, cost));
        }
    }
}

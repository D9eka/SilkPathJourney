using System;
using System.Collections.Generic;

namespace Internal.Scripts.Utils
{
    public static class BfsUtility
    {
        public static int ShortestDistance<T>(T start, T end, Func<T, IEnumerable<T>> getNeighbors) where T : notnull
        {
            if (EqualityComparer<T>.Default.Equals(start, end)) return 0;
            var visited = new HashSet<T> { start };
            var queue = new Queue<(T node, int depth)>();
            queue.Enqueue((start, 0));
            while (queue.Count > 0)
            {
                var (current, depth) = queue.Dequeue();
                foreach (T neighbor in getNeighbors(current))
                {
                    if (EqualityComparer<T>.Default.Equals(neighbor, end))
                        return depth + 1;
                    if (visited.Add(neighbor))
                        queue.Enqueue((neighbor, depth + 1));
                }
            }
            return 0;
        }

        public static List<T> NodesInRadius<T>(T start, int maxHops,
            Func<T, IEnumerable<T>> getNeighbors, Func<T, bool> filter = null) where T : notnull
        {
            var result = new List<T>();
            var visited = new HashSet<T> { start };
            var queue = new Queue<(T node, int hops)>();
            queue.Enqueue((start, 0));
            while (queue.Count > 0)
            {
                var (node, hops) = queue.Dequeue();
                if (hops >= maxHops) continue;
                foreach (T neighbor in getNeighbors(node))
                {
                    if (!visited.Add(neighbor)) continue;
                    if (filter == null || filter(neighbor))
                        result.Add(neighbor);
                    queue.Enqueue((neighbor, hops + 1));
                }
            }
            return result;
        }
    }
}

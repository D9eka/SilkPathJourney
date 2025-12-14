using System.Collections.Generic;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Pathfinder;

namespace Internal.Scripts.Road.Paths
{
    public class DijkstraRoadPathfinderStrategy : IRoadPathfinderStrategy
    {
        public List<RoadNode> FindPathNodes(RoadGraph graph, RoadNode start, RoadNode goal)
        {
            var result = new List<RoadNode>();
            if (graph == null || start == null || goal == null) return result;
            if (start == goal)
            {
                result.Add(start);
                return result;
            }

            var dist = new Dictionary<RoadNode, float>(graph.Nodes.Count);
            var prev = new Dictionary<RoadNode, RoadNode>(graph.Nodes.Count);
            var visited = new HashSet<RoadNode>();

            int tie = 0;
            var pq = new SortedSet<State>(new StateComparer());
            void Push(RoadNode n, float d)
            {
                pq.Add(new State(d, tie++, n));
            }

            foreach (var kv in graph.Nodes)
                dist[kv.Value] = float.PositiveInfinity;

            dist[start] = 0f;
            Push(start, 0f);

            while (pq.Count > 0)
            {
                var curState = pq.Min;
                pq.Remove(curState);

                RoadNode cur = curState.Node;
                if (visited.Contains(cur)) continue;
                visited.Add(cur);

                if (cur == goal) break;

                float curDist = dist[cur];
                foreach (var edge in cur.OutgoingEdges)
                {
                    RoadNode next = edge.To;
                    float nd = curDist + edge.Length;

                    if (nd < dist.GetValueOrDefault(next, float.PositiveInfinity))
                    {
                        dist[next] = nd;
                        prev[next] = cur;
                        Push(next, nd);
                    }
                }
            }

            if (!prev.ContainsKey(goal))
                return result;

            var stack = new Stack<RoadNode>();
            RoadNode it = goal;
            stack.Push(it);

            while (it != start)
            {
                it = prev[it];
                stack.Push(it);
            }

            result.AddRange(stack);
            return result;
        }

        private readonly struct State
        {
            public readonly float Dist;
            public readonly int Tie;
            public readonly RoadNode Node;

            public State(float dist, int tie, RoadNode node)
            {
                Dist = dist;
                Tie = tie;
                Node = node;
            }
        }

        private sealed class StateComparer : IComparer<State>
        {
            public int Compare(State a, State b)
            {
                int c = a.Dist.CompareTo(b.Dist);
                if (c != 0) return c;
                return a.Tie.CompareTo(b.Tie);
            }
        }
    }
}

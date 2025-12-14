using System;
using System.Collections.Generic;
using Internal.Scripts.Road.Graph;

namespace Internal.Scripts.Road.Paths
{
    public sealed class RoutePathCursor : PathCursorBase
    {
        private readonly List<RoadEdge> _edges = new();
        private int _edgeIndex;

        public event Action Finished;

        public RoutePathCursor(IReadOnlyList<RoadNode> route)
            : base(BuildFirstEdge(route, out var edges))
        {
            _edges.Clear();
            _edges.AddRange(edges);
            _edgeIndex = 0;
        }
        
        public void SetRoute(IReadOnlyList<RoadNode> route, bool resume = true)
        {
            var first = BuildFirstEdge(route, out var edges);

            _edges.Clear();
            _edges.AddRange(edges);
            _edgeIndex = 0;
            
            ResetToEdge(first, resume);
        }

        protected override void OnNodeReached(RoadNode node)
        {
            _edgeIndex++;

            if (_edgeIndex >= _edges.Count)
            {
                Finish();
                Finished?.Invoke();
                return;
            }

            SwitchToEdge(_edges[_edgeIndex]);
        }

        private static RoadEdge BuildFirstEdge(IReadOnlyList<RoadNode> route, out List<RoadEdge> edges)
        {
            if (route == null || route.Count < 2)
                throw new ArgumentException("Route must contain at least 2 nodes.", nameof(route));

            edges = new List<RoadEdge>(route.Count - 1);

            for (int i = 0; i < route.Count - 1; i++)
            {
                RoadNode from = route[i];
                RoadNode to = route[i + 1];

                RoadEdge edge = FindEdge(from, to);
                if (edge == null)
                    throw new InvalidOperationException($"No edge {from.Id} -> {to.Id}");

                edges.Add(edge);
            }

            return edges[0];
        }

        private static RoadEdge FindEdge(RoadNode from, RoadNode to)
        {
            foreach (var e in from.OutgoingEdges)
                if (e.To == to)
                    return e;
            return null;
        }
    }
}

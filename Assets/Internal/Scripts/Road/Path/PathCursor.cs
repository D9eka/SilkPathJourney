using System;
using Internal.Scripts.Road.Graph;
using UnityEngine;

namespace Internal.Scripts.Road.Paths
{
    public sealed class PathCursor
    {
        public RoadEdge CurrentEdge { get; private set; }
        public float DistanceOnEdge { get; private set; }

        public RoadNode CurrentNode => DistanceOnEdge <= 0f
            ? CurrentEdge.From
            : (DistanceOnEdge >= CurrentEdge.Length ? CurrentEdge.To : null);

        public bool IsAtEndOfEdge => DistanceOnEdge >= CurrentEdge.Length;

        public Vector3 Position => CurrentEdge.Path.GetPosition(DistanceOnEdge);
        public Vector3 Forward => CurrentEdge.Path.GetTangent(DistanceOnEdge);

        public event Action<RoadNode> JunctionReached;

        public PathCursor(RoadEdge startEdge)
        {
            CurrentEdge = startEdge ?? throw new ArgumentNullException(nameof(startEdge));
            DistanceOnEdge = 0f;
        }
        
        public bool Advance(float deltaDistance)
        {
            if (CurrentEdge == null || Mathf.Approximately(deltaDistance, 0f))
                return false;

            float remaining = deltaDistance;
            bool reachedJunction = false;

            while (remaining > 0f && CurrentEdge != null && !reachedJunction)
            {
                float distToEnd = CurrentEdge.Length - DistanceOnEdge;

                if (remaining < distToEnd)
                {
                    DistanceOnEdge += remaining;
                    remaining = 0f;
                }
                else
                {
                    DistanceOnEdge = CurrentEdge.Length;
                    remaining -= distToEnd;

                    RoadNode node = CurrentEdge.To;
                    int count = node.OutgoingEdges.Count;

                    if (count == 0)
                    {
                        remaining = 0f;
                    }
                    else if (count == 1)
                    {
                        RoadEdge nextEdge = node.OutgoingEdges[0];
                        CurrentEdge = nextEdge;
                        DistanceOnEdge = 0f;
                    }
                    else
                    {
                        JunctionReached?.Invoke(node);
                        reachedJunction = true;
                        remaining = 0f;
                    }
                }
            }

            return reachedJunction;
        }
        
        public void SwitchToEdge(RoadEdge nextEdge)
        {
            if (nextEdge == null) throw new ArgumentNullException(nameof(nextEdge));
            if (nextEdge.From != CurrentEdge.To)
                throw new InvalidOperationException("Next edge must start at current node.");

            CurrentEdge = nextEdge;
            DistanceOnEdge = 0f;
        }
    }
}

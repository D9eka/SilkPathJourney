using System;
using Internal.Scripts.Road.Graph;
using UnityEngine;

namespace Internal.Scripts.Road.Paths
{
    public abstract class PathCursorBase
    {
        public RoadEdge CurrentEdge { get; protected set; }
        public float DistanceOnEdge { get; protected set; }

        public RoadNode CurrentNode { get; private set; }

        public bool IsAtEndOfEdge => CurrentEdge != null && DistanceOnEdge >= CurrentEdge.Length;
        public bool IsFinished => CurrentEdge == null;
        public bool IsStopped { get; private set; }

        public Vector3 Position => CurrentEdge != null ? CurrentEdge.Path.GetPosition(DistanceOnEdge) : Vector3.zero;
        public Vector3 Forward  => CurrentEdge != null ? CurrentEdge.Path.GetTangent(DistanceOnEdge) : Vector3.right;

        public event Action<RoadNode> NodeReached;
        public event Action<RoadNode> CurrentNodeChanged;

        protected PathCursorBase(RoadEdge startEdge)
        {
            ResetToEdge(startEdge, resume: true);
        }

        public void Stop() => IsStopped = true;
        public void Resume() => IsStopped = false;

        public void ResetToEdge(RoadEdge startEdge, bool resume = true)
        {
            if (startEdge == null) throw new ArgumentNullException(nameof(startEdge));

            CurrentEdge = startEdge;
            DistanceOnEdge = 0f;

            if (resume) Resume();

            SetCurrentNode(CurrentEdge.From);
        }

        public bool Advance(float deltaDistance)
        {
            if (IsStopped) return false;
            if (CurrentEdge == null || Mathf.Approximately(deltaDistance, 0f))
                return false;

            float remaining = deltaDistance;
            bool reachedNode = false;

            while (remaining > 0f && CurrentEdge != null && !IsStopped)
            {
                float distToEnd = CurrentEdge.Length - DistanceOnEdge;

                if (remaining < distToEnd)
                {
                    DistanceOnEdge += remaining;
                    remaining = 0f;

                    SetCurrentNode(null);
                }
                else
                {
                    DistanceOnEdge = CurrentEdge.Length;
                    remaining -= distToEnd;

                    RoadNode node = CurrentEdge.To;

                    // теперь в ноде
                    SetCurrentNode(node);

                    NodeReached?.Invoke(node);
                    reachedNode = true;

                    OnNodeReached(node);
                }
            }

            return reachedNode;
        }

        protected abstract void OnNodeReached(RoadNode node);

        protected void SwitchToEdge(RoadEdge nextEdge)
        {
            if (nextEdge == null) throw new ArgumentNullException(nameof(nextEdge));

            CurrentEdge = nextEdge;
            DistanceOnEdge = 0f;

            SetCurrentNode(CurrentEdge.From);
        }

        protected void Finish(bool clearNode = false)
        {
            CurrentEdge = null;
            DistanceOnEdge = 0f;

            if (clearNode)
                SetCurrentNode(null);
        }

        private void SetCurrentNode(RoadNode node)
        {
            if (ReferenceEquals(CurrentNode, node)) return;
            CurrentNode = node;
            CurrentNodeChanged?.Invoke(CurrentNode);
        }
    }
}

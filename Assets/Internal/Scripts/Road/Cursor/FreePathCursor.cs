using System;
using Internal.Scripts.Road.Graph;

namespace Internal.Scripts.Road.Cursor
{
    public sealed class FreePathCursor : PathCursorBase
    {
        public event Action<RoadNode> JunctionReached;

        public FreePathCursor(RoadEdge startEdge) : base(startEdge) { }

        public void SetStartEdge(RoadEdge startEdge, bool resume = true)
        {
            ResetToEdge(startEdge, resume);
        }

        protected override void OnNodeReached(RoadNode node)
        {
            int count = node.OutgoingEdges.Count;

            if (count == 0)
            {
                Finish();
            }
            else if (count == 1)
            {
                SwitchToEdge(node.OutgoingEdges[0]);
            }
            else
            {
                Stop();
                JunctionReached?.Invoke(node);
            }
        }

        public void ChooseNext(RoadEdge nextEdge)
        {
            SwitchToEdge(nextEdge);
            Resume();
        }
    }
}
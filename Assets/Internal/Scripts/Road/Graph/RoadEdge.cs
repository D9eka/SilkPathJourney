using Internal.Scripts.Road.Paths;

namespace Internal.Scripts.Road.Graph
{
    public sealed class RoadEdge
    {
        public RoadNode From { get; }
        public RoadNode To { get; }
        public ICurvePath Path { get; }

        public float Length => Path.Length;

        public RoadEdge(RoadNode from, RoadNode to, ICurvePath path)
        {
            From = from;
            To = to;
            Path = path;
        }

        public override string ToString() => $"Edge {From.Id} -> {To.Id} (len = {Length})";
    }
}
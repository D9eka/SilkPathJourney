using Internal.Scripts.Road.Core;
using Internal.Scripts.World.Roads;

namespace Internal.Scripts.Road.Graph
{
    public sealed class RoadSegmentData
    {
        public RoadSegmentId Id { get; }
        public RoadRuntime Runtime { get; }
        public RoadData Data { get; }
        public float LengthMeters { get; }
        public bool IsBidirectional { get; }
        public float SpeedMultiplier { get; }

        public RoadSegmentData(RoadSegmentId id, RoadRuntime runtime, RoadData data, float lengthMeters, bool isBidirectional, float speedMultiplier)
        {
            Id = id;
            Runtime = runtime;
            Data = data;
            LengthMeters = lengthMeters;
            IsBidirectional = isBidirectional;
            SpeedMultiplier = speedMultiplier;
        }
    }
}

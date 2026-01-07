using Internal.Scripts.Road.Components;
using Internal.Scripts.World.Roads;

namespace Internal.Scripts.Road.Path
{
    public static class RoadLaneUtility
    {
        public static float ComputeLaneOffsetMeters(RoadData data, RoadLane lane, float lateralOffset)
        {
            float offset = 0f;
            if (data != null && data.LaneCount > 1 && lane != RoadLane.Center)
            {
                float w = data.LaneWidth > 0f ? data.LaneWidth : 3.5f;
                offset = lane == RoadLane.Right ? w * 0.5f : -w * 0.5f;
            }

            return offset + lateralOffset;
        }
    }
}

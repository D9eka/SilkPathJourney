using System;
using Internal.Scripts.Road;
using Internal.Scripts.Road.Components;

namespace Internal.Scripts.Npc.Core
{
    [Serializable]
    public class NpcConfig
    {
        public float SpeedMetersPerSecond = 5f;
        public RoadLane Lane = RoadLane.Right;
        public float LateralOffsetMeters = 0f;
    }
}

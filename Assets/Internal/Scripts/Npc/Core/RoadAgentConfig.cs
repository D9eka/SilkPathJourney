using System;
using Internal.Scripts.Road.Core;
using UnityEngine.Serialization;

namespace Internal.Scripts.Npc.Core
{
    [Serializable]
    public class RoadAgentConfig
    {
        public float SpeedMetersPerDay = 5f;
        public RoadLane Lane = RoadLane.Right;
        public float LateralOffsetMeters = 0f;
    }
}

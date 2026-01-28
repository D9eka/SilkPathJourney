using System;

namespace Internal.Scripts.World.Editor.RoadImporter.DTO
{
    [Serializable] 
    public class Meta
    {
        public int LaneCount;
        public float LaneWidth;
        public float SpeedMul;
        public bool Bidirectional;
        public float SampleStepMeters;
    }
}
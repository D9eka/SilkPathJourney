using System;

namespace Internal.Scripts.Import.Editor.Roads.DTO
{
    [Serializable]
    public class Meta
    {
        public int LaneCount;
        public float LaneWidth;
        public float SpeedMul;
        public bool Bidirectional;
        public float SampleStepMeters;
        public bool Hidden;
    }
}

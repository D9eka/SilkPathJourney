using System;

namespace Internal.Scripts.Economy.Cities
{
    [Serializable]
    public struct ThresholdModifierRule
    {
        public int Threshold;
        public float Modifier;
        public ComparisonType Comparison;
    }
}

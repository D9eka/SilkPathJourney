using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Input
{
    [Serializable]
    public sealed class DodgeInputConfig : HazardInputConfigBase
    {
        public override HazardInputType InputType => HazardInputType.Dodge;

        [field: SerializeField] public float CartSpeed { get; private set; } = 100f;
        [field: SerializeField] public float ReactionMargin { get; private set; } = 120f;
        [field: SerializeField] public float EscapeMargin { get; private set; } = 80f;
        [field: SerializeField] public float SegmentMargin { get; private set; } = 120f;
    }
}

using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Input
{
    [Serializable]
    public sealed class HoldClickInputConfig : HazardInputConfigBase
    {
        public override HazardInputType InputType => HazardInputType.HoldClick;

        [field: SerializeField] public float HoldDuration { get; private set; } = 1.5f;
        [field: SerializeField] public float SlideSpeed { get; private set; } = 60f;
    }
}

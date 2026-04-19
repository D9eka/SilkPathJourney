using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Input
{
    [Serializable]
    public sealed class ClickInputConfig : HazardInputConfigBase
    {
        public override HazardInputType InputType => HazardInputType.Click;

        [field: SerializeField] public float CartSpeed { get; private set; } = 100f;
        [field: SerializeField] public bool ReverseDirection { get; private set; }
    }
}

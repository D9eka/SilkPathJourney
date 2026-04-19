using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Input
{
    [Serializable]
    public sealed class DownInputConfig : HazardInputConfigBase
    {
        public override HazardInputType InputType => HazardInputType.Down;

        [field: SerializeField] public float CartSpeed { get; private set; } = 100f;
    }
}

using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Input
{
    [Serializable]
    public sealed class TimingClickInputConfig : IHazardInputConfig
    {
        public HazardInputType InputType => HazardInputType.TimingClick;

        [field: SerializeField] public float PulseSpeed { get; private set; } = 2f;
        [field: SerializeField] public float CalmThreshold { get; private set; } = 0.45f;
    }
}

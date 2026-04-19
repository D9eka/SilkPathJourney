using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Input
{
    [Serializable]
    public sealed class WindResistInputConfig : IHazardInputConfig
    {
        public HazardInputType InputType => HazardInputType.WindResist;

        [field: SerializeField] public float WindSpeed { get; private set; } = 80f;
        [field: SerializeField] public float ClickPush { get; private set; } = 30f;
        [field: SerializeField] public bool WindChangesDirection { get; private set; }
    }
}

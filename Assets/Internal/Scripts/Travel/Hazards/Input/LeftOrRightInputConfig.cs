using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Input
{
    [Serializable]
    public sealed class LeftOrRightInputConfig : IHazardInputConfig
    {
        public HazardInputType InputType => HazardInputType.LeftOrRight;

        [field: SerializeField] public float MoveSpeed { get; private set; } = 150f;
    }
}

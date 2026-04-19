using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Input
{
    [Serializable]
    public sealed class LeftOrRightInputConfig : HazardInputConfigBase
    {
        public override HazardInputType InputType => HazardInputType.LeftOrRight;

        [field: SerializeField] public float MoveSpeed { get; private set; } = 150f;
    }
}

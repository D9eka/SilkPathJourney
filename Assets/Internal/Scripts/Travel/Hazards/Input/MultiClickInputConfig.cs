using System;
using Internal.Scripts.Travel.Hazards.Data;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Input
{
    [Serializable]
    public sealed class MultiClickInputConfig : HazardInputConfigBase
    {
        public override HazardInputType InputType => HazardInputType.MultiClick;

        [field: SerializeField] public int MinClicks { get; private set; } = 2;
        [field: SerializeField] public int MaxClicks { get; private set; } = 5;
        [field: SerializeField] public EnemyPool EnemyPool { get; private set; }
    }
}

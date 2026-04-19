using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Input
{
    [Serializable]
    public sealed class MultiClickInputConfig : IHazardInputConfig
    {
        public HazardInputType InputType => HazardInputType.MultiClick;

        [field: SerializeField] public int RequiredClicks { get; private set; } = 3;
    }
}

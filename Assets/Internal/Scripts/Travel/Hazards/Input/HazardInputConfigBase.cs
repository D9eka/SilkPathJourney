using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Travel.Hazards.Input
{
    [Serializable]
    public abstract class HazardInputConfigBase : IHazardInputConfig
    {
        public abstract HazardInputType InputType { get; }
        [field: SerializeField] public LocalizedString Hint { get; private set; }
    }
}

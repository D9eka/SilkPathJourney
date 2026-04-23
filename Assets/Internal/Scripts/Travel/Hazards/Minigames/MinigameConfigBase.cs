using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Travel.Hazards.Minigames
{
    [Serializable]
    public abstract class MinigameConfigBase : IMinigameConfig
    {
        [field: SerializeField] public LocalizedString Hint { get; private set; }
    }
}

using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Minigames
{
    [Serializable]
    public sealed class SlopeMinigameConfig : MinigameConfigBase
    {
        [field: SerializeField] public float HoldDuration { get; private set; } = 1.5f;
        [field: SerializeField] public float SlideSpeed { get; private set; } = 60f;
    }
}

using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Minigames
{
    [Serializable]
    public sealed class RiverMinigameConfig : MinigameConfigBase
    {
        [field: SerializeField] public float PulseSpeed { get; private set; } = 2f;
        [field: SerializeField] public float CalmThreshold { get; private set; } = 0.45f;
    }
}

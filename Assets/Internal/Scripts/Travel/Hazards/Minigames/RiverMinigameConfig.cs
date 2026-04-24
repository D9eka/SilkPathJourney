using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Minigames
{
    [Serializable]
    public sealed class RiverMinigameConfig : MinigameConfigBase
    {
        [field: SerializeField] public float MinPulseSpeed { get; private set; } = 1f;
        [field: SerializeField] public float MaxPulseSpeed { get; private set; } = 3.5f;
        [field: SerializeField] public float CalmThreshold { get; private set; } = 0.45f;
        [field: SerializeField] public float StepDuration { get; private set; } = 1.5f;
        [field: SerializeField] public float SinkMoveDuration { get; private set; } = 0.8f;
        [field: SerializeField] public float SinkScaleDuration { get; private set; } = 0.6f;
        [field: SerializeField] public float CompletionDelay { get; private set; } = 0.4f;
    }
}

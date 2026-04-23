using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Minigames
{
    [Serializable]
    public sealed class SandstormMinigameConfig : MinigameConfigBase
    {
        [field: SerializeField] public float CartSpeed { get; private set; } = 80f;
        [field: SerializeField] public float ClickPush { get; private set; } = 30f;
        [field: SerializeField, Range(0f, 1f)] public float DangerWidthRatioMin { get; private set; } = 0.15f;
        [field: SerializeField, Range(0f, 1f)] public float DangerWidthRatioMax { get; private set; } = 0.4f;
    }
}

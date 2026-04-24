using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Minigames
{
    [Serializable]
    public sealed class DodgeMinigameConfig : MinigameConfigBase
    {
        [field: SerializeField] public float CartSpeed { get; private set; } = 100f;
        [field: SerializeField] public float ReactionMargin { get; private set; } = 120f;
        [field: SerializeField] public float EscapeMargin { get; private set; } = 80f;
        [field: SerializeField] public float SegmentMargin { get; private set; } = 120f;
    }
}

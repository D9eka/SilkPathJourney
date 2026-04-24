using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Minigames
{
    [Serializable]
    public sealed class RockfallMinigameConfig : MinigameConfigBase
    {
        [field: SerializeField] public float MoveSpeed { get; private set; } = 150f;
    }
}

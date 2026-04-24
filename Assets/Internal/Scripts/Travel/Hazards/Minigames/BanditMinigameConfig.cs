using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Minigames
{
    [Serializable]
    public sealed class BanditMinigameConfig : MinigameConfigBase
    {
        [field: SerializeField] public float CartSpeed { get; private set; } = 100f;
    }
}

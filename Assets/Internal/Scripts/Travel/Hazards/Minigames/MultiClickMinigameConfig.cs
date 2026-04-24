using System;
using Internal.Scripts.Travel.Hazards.Data;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Minigames
{
    [Serializable]
    public sealed class MultiClickMinigameConfig : MinigameConfigBase
    {
        [field: SerializeField] public int MinClicks { get; private set; } = 2;
        [field: SerializeField] public int MaxClicks { get; private set; } = 5;
        [field: SerializeField] public EnemyPool EnemyPool { get; private set; }
    }
}

using UnityEngine;

namespace Internal.Scripts.Config
{
    [CreateAssetMenu(menuName = "SPJ/Game Balance Config", fileName = "GameBalanceConfig")]
    public sealed class GameBalanceConfig : ScriptableObject
    {
        [Header("Day Progression")]
        [field: SerializeField] public float SecondsPerDay { get; private set; } = 20f;

        [Header("Events")]
        [field: SerializeField] public int DaysBetweenEvents { get; private set; } = 3;

    }
}

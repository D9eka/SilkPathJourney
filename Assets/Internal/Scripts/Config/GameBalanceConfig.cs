using UnityEngine;

namespace Internal.Scripts.Config
{
    [CreateAssetMenu(menuName = "SPJ/Game Balance Config", fileName = "GameBalanceConfig")]
    public sealed class GameBalanceConfig : ScriptableObject
    {
        [Header("Day Progression")]
        [field: SerializeField] public float SecondsPerDay { get; private set; } = 20f;

        [Header("Events")]
        [field: SerializeField] public int DaysBetweenMajorEvents { get; private set; } = 5;
        [field: SerializeField] public int DaysBetweenMinorEvents { get; private set; } = 2;
        [field: SerializeField] public float ToastDisplayDuration { get; private set; } = 4f;

        [Header("Resource Limits")]
        [field: SerializeField] public float MaxFood { get; private set; } = 100f;
        [field: SerializeField] public float MaxDanger { get; private set; } = 100f;
    }
}

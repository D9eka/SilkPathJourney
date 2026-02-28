using Internal.Scripts.World.State;
using UnityEngine;

namespace Internal.Scripts.Config
{
    [CreateAssetMenu(menuName = "SPJ/Time Speed Config", fileName = "TimeSpeedConfig")]
    public sealed class TimeSpeedConfig : ScriptableObject
    {
        [field: SerializeField] public float NormalMultiplier { get; private set; } = 1f;
        [field: SerializeField] public float FastMultiplier { get; private set; } = 2f;
        [field: SerializeField] public float VeryFastMultiplier { get; private set; } = 3f;

        public float GetMultiplier(TimeSpeed speed) => speed switch
        {
            TimeSpeed.Paused => 0f,
            TimeSpeed.Normal => NormalMultiplier,
            TimeSpeed.Fast => FastMultiplier,
            TimeSpeed.VeryFast => VeryFastMultiplier,
            _ => 1f
        };
    }
}

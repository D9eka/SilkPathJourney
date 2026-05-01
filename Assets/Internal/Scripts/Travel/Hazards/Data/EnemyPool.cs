using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Data
{
    [CreateAssetMenu(menuName = "SPJ/Hazards/Enemy Pool", fileName = "New EnemyPool")]
    public sealed class EnemyPool : ScriptableObject
    {
        [field: SerializeField] public EnemyVisual[] Visuals { get; private set; }
    }
}

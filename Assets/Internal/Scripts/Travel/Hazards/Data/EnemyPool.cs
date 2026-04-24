using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Data
{
    [CreateAssetMenu(menuName = "SilkPath/Hazards/Enemy Pool", fileName = "Pool_")]
    public sealed class EnemyPool : ScriptableObject
    {
        [field: SerializeField] public EnemyVisual[] Visuals { get; private set; }
    }
}

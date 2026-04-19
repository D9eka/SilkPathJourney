using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards
{
    [CreateAssetMenu(menuName = "SPJ/Travel/Hazard Database", fileName = "HazardDatabase")]
    public sealed class HazardDatabase : ScriptableObject
    {
        [field: SerializeField] public List<HazardData> Hazards { get; private set; } = new();
    }
}

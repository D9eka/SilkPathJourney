using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Travel.Pickups
{
    [CreateAssetMenu(menuName = "SPJ/Travel/Pickup Database", fileName = "PickupDatabase")]
    public sealed class PickupDatabase : ScriptableObject
    {
        [field: SerializeField] public List<PickupData> Pickups { get; private set; } = new();
    }
}

using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Events.Data;
using UnityEngine;

namespace Internal.Scripts.Travel.Pickups
{
    [CreateAssetMenu(menuName = "SPJ/Travel/Pickup Data", fileName = "PickupData")]
    public sealed class PickupData : ScriptableObject
    {
        [field: SerializeField] public PickupType Type { get; private set; }
        [field: SerializeField] public GameObject Prefab { get; private set; }
        [field: SerializeField] public List<EventOutcomeEntry> Rewards { get; private set; } = new();
        [field: SerializeField] public Biome BiomeFilter { get; private set; } = Biome.Unknown;
        [field: SerializeField] public float Weight { get; private set; } = 1f;
    }
}

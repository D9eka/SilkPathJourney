using UnityEngine;

namespace Internal.Scripts.Economy.Simulation
{
    [CreateAssetMenu(menuName = "SPJ/Economy/Simulation Settings", fileName = "EconomySimulationSettings")]
    public sealed class EconomySimulationSettings : ScriptableObject
    {
        [field: SerializeField] public float InitialStockRatio { get; private set; } = 0.9f;
        [field: SerializeField] public float InitialStockVariationPct { get; private set; } = 0.15f;
    }
}

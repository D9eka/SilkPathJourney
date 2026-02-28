using UnityEngine;

namespace Internal.Scripts.Economy.Simulation
{
    [CreateAssetMenu(menuName = "SPJ/Economy/Simulation Settings", fileName = "EconomySimulationSettings")]
    public sealed class EconomySimulationSettings : ScriptableObject
    {
        [field: Header("Initial Stock")]
        [field: SerializeField] public float InitialStockRatio { get; private set; } = 0.9f;
        [field: SerializeField] public float InitialStockVariationPct { get; private set; } = 0.15f;

        [field: Header("Price Dynamics")]
        [field: SerializeField] public float PriceScarcityStrength { get; private set; } = 0.35f;
        [field: SerializeField] public float PriceMultiplierMin { get; private set; } = 0.70f;
        [field: SerializeField] public float PriceMultiplierMax { get; private set; } = 1.35f;
    }
}

using System.Collections.Generic;
using Internal.Scripts.Attributes;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Travel.Hazards.Minigames;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Travel.Hazards
{
    [CreateAssetMenu(menuName = "SPJ/Travel/Hazard Data", fileName = "HazardData")]
    public sealed class HazardData : ScriptableObject
    {
        [field: SerializeField] public HazardType Type { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public LocalizedString WarningText { get; private set; }
        [field: SerializeField] public float TimeLimit { get; private set; } = 3f;
        [field: SerializeField] public GameObject MinigamePrefab { get; private set; }
        [SerializeReference, SubclassSelector] public IMinigameConfig MinigameConfig;
        [field: SerializeField] public Biome BiomeFilter { get; private set; } = Biome.Unknown;
        [field: SerializeField] public List<EventOutcomeEntry> SuccessOutcomes { get; private set; } = new();
        [field: SerializeField] public List<EventOutcomeEntry> FailOutcomes { get; private set; } = new();
    }
}

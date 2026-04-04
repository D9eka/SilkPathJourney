using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using UnityEngine;

namespace Internal.Scripts.Economy.Cities.Smuggling
{
    [CreateAssetMenu(menuName = "SPJ/NPC/Smuggling Detection Settings")]
    public sealed class SmugglingDetectionSettings : ScriptableObject
    {
        [SerializeField] private float _baseChance = 0.15f;
        [SerializeField] private float _caughtMultiplier = 2.0f;
        [SerializeField] private List<ThresholdModifierRule> _reputationRules = new()
        {
            new ThresholdModifierRule { Threshold = 10, Modifier = 1.5f, Comparison = ComparisonType.Below },
            new ThresholdModifierRule { Threshold = 75, Modifier = 0.5f, Comparison = ComparisonType.Above },
        };
        [SerializeField] private List<ThresholdModifierRule> _skillRules = new()
        {
            new ThresholdModifierRule { Threshold = 80, Modifier = 0.5f, Comparison = ComparisonType.Above },
            new ThresholdModifierRule { Threshold = 50, Modifier = 0.7f, Comparison = ComparisonType.Above },
        };

        public float BaseChance => _baseChance;
        public float CaughtMultiplier => _caughtMultiplier;
        public IReadOnlyList<ThresholdModifierRule> ReputationRules => _reputationRules;
        public IReadOnlyList<ThresholdModifierRule> SkillRules => _skillRules;
    }
}

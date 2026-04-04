using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;

namespace Internal.Scripts.Economy.Cities.Smuggling
{
    public sealed class SmugglingModifierCalculator
    {
        private readonly IReadOnlyList<ThresholdModifierRule> _reputationRules;
        private readonly IReadOnlyList<ThresholdModifierRule> _skillRules;

        public SmugglingModifierCalculator(SmugglingDetectionSettings settings)
        {
            _reputationRules = settings.ReputationRules;
            _skillRules = settings.SkillRules;
        }

        public float GetReputationModifier(int reputation) => EvaluateRules(_reputationRules, reputation);
        public float GetSkillModifier(int tradeSkill) => EvaluateRules(_skillRules, tradeSkill);

        public static float EvaluateRules(IReadOnlyList<ThresholdModifierRule> rules, int value, float defaultValue = 1.0f)
        {
            foreach (var rule in rules)
            {
                bool match = rule.Comparison switch
                {
                    ComparisonType.Above => value > rule.Threshold,
                    ComparisonType.Below => value < rule.Threshold,
                    ComparisonType.Equal => value == rule.Threshold,
                    _ => false
                };
                if (match) return rule.Modifier;
            }
            return defaultValue;
        }
    }
}

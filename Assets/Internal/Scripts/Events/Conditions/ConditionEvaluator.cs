using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using UnityEngine;

namespace Internal.Scripts.Events.Conditions
{
    public class ConditionEvaluator
    {
        private readonly Dictionary<EventConditionType, IConditionEvaluator> _evaluators = new();

        public ConditionEvaluator(
            ResourceEvaluator resource,
            InventoryEvaluator inventory,
            CartEvaluator cart,
            LocationEvaluator location,
            CompanionEvaluator companion,
            MinSkillEvaluator skill,
            MinLanguageProficiencyEvaluator language,
            InCampEvaluator inCamp,
            MinCompanionsEvaluator minCompanions,
            CityEvaluator city,
            ReputationConditionEvaluator reputation,
            MoraleConditionEvaluator morale)
        {
            Register(resource);
            Register(inventory);
            Register(cart);
            Register(location);
            Register(companion);
            Register(skill);
            Register(language);
            Register(inCamp);
            Register(minCompanions);
            Register(city);
            Register(reputation);
            Register(morale);
        }

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            if (_evaluators.TryGetValue(condition.Type, out var evaluator))
                return evaluator.Evaluate(condition, resources);

            Debug.LogWarning($"[SPJ Events] No condition evaluator for {condition.Type}");
            return false;
        }

        private void Register(IConditionEvaluator evaluator)
        {
            foreach (var type in evaluator.SupportedTypes)
                _evaluators[type] = evaluator;
        }
    }
}

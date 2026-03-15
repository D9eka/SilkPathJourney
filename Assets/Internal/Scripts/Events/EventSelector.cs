using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy;
using Internal.Scripts.Events.Conditions;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Outcomes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Internal.Scripts.Events
{
    public class EventSelector
    {
#if UNITY_EDITOR
        public static string DebugEventPrefix { get; set; }
#endif
        private readonly EventDatabase _eventDatabase;
        private readonly ConditionEvaluator _conditionEvaluator;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly OutcomeApplier _outcomeApplier;

        public EventSelector(
            EventDatabase eventDatabase,
            ConditionEvaluator conditionEvaluator,
            PlayerResourceRepository resourceRepository,
            OutcomeApplier outcomeApplier)
        {
            _eventDatabase = eventDatabase;
            _conditionEvaluator = conditionEvaluator;
            _resourceRepository = resourceRepository;
            _outcomeApplier = outcomeApplier;
        }

        public EventData SelectEvent(bool minor)
        {
            if (_eventDatabase == null || _eventDatabase.Events == null || _eventDatabase.Events.Count == 0)
                return null;

            List<EventData> eligible = new();
            float totalWeight = 0f;

            foreach (var evt in _eventDatabase.Events)
            {
                if (!IsEligible(evt, minor))
                    continue;

                eligible.Add(evt);
                totalWeight += evt.Weight;
            }

            if (eligible.Count == 0)
                return null;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            foreach (var evt in eligible)
            {
                cumulative += evt.Weight;
                if (roll <= cumulative)
                    return evt;
            }

            return eligible[eligible.Count - 1];
        }

        private bool IsEligible(EventData evt, bool minor)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(DebugEventPrefix) && !evt.Id.StartsWith(DebugEventPrefix))
                return false;
#endif
            return evt.IsMinor == minor && evt.Weight > 0f &&
                   CheckConditions(evt.Conditions) && HasAvailableChoices(evt);
        }

        public bool HasAvailableChoices(EventData eventData)
        {
            if (eventData.Choices == null || eventData.Choices.Count == 0)
                return false;

            return eventData.Choices.Any(c =>
                (c.Conditions == null || c.Conditions.Count == 0 ||
                 CheckConditions(c.Conditions))
                && _outcomeApplier.CanAffordAll(c.Outcomes));
        }

        public bool CheckConditions(List<EventCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0)
                return true;

            var resources = _resourceRepository.Current;
            return conditions.All(c => _conditionEvaluator.Evaluate(c, resources));
        }
    }
}

using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;

namespace Internal.Scripts.Events.Conditions
{
    public class MoraleConditionEvaluator : IConditionEvaluator
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.MinMorale,
            EventConditionType.MaxMorale
        };

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            return condition.Type switch
            {
                EventConditionType.MinMorale => resources.Morale >= condition.Value,
                EventConditionType.MaxMorale => resources.Morale <= condition.Value,
                _ => false
            };
        }
    }
}

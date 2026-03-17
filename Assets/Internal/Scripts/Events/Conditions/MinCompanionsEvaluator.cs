using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;

namespace Internal.Scripts.Events.Conditions
{
    public class MinCompanionsEvaluator : IConditionEvaluator
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.MinCompanions
        };

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            return resources.Companions != null
                && resources.Companions.Count(c => !c.IsInjured) >= (int)condition.Value;
        }
    }
}

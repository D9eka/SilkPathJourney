using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;

namespace Internal.Scripts.Events.Conditions
{
    public class CompanionEvaluator : IConditionEvaluator
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.HasCompanion
        };

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            if (resources.Companions == null)
                return false;

            if (string.IsNullOrEmpty(condition.Param))
                return resources.Companions.Count > 0;

            return resources.Companions.Any(c =>
                string.Equals(c.TypeId, condition.Param, StringComparison.OrdinalIgnoreCase) && !c.IsInjured);
        }
    }
}

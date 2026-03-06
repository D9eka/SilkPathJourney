using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;

namespace Internal.Scripts.Events.Conditions
{
    public interface IConditionEvaluator
    {
        IEnumerable<EventConditionType> SupportedTypes { get; }
        bool Evaluate(EventCondition condition, PlayerResourceState resources);
    }
}

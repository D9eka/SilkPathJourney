using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;

namespace Internal.Scripts.Events.Conditions
{
    public interface IConditionHandler
    {
        IEnumerable<EventConditionType> SupportedTypes { get; }
        bool Evaluate(EventCondition condition, PlayerResourceState resources);
    }
}

using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.UI.Components;

namespace Internal.Scripts.Events.Outcomes
{
    public interface IOutcomeHandler
    {
        IEnumerable<EventOutcomeType> SupportedTypes { get; }
        ResourceType? GetAffectedResource(EventOutcomeType type);
        void Apply(EventOutcomeEntry entry);
    }
}

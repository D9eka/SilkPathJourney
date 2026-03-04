using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.UI.Components;

namespace Internal.Scripts.Events.Outcomes
{
    public interface IOutcomeApplier
    {
        IEnumerable<EventOutcomeType> SupportedTypes { get; }
        ResourceType? GetAffectedResource(EventOutcomeType type);
        void Apply(EventOutcomeEntry entry);
        bool CanAfford(EventOutcomeType type, float netValue);
    }
}

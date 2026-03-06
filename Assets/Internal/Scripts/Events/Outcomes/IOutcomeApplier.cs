using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;

namespace Internal.Scripts.Events.Outcomes
{
    public interface IOutcomeApplier
    {
        IEnumerable<EventOutcomeType> SupportedTypes { get; }
        void Apply(EventOutcomeEntry entry);
    }
}

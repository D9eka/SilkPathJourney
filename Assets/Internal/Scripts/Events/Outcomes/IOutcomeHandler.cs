using System.Collections.Generic;
using Internal.Scripts.Events.Data;

namespace Internal.Scripts.Events.Outcomes
{
    public interface IOutcomeHandler
    {
        IEnumerable<EventOutcomeType> SupportedTypes { get; }
        void Apply(EventOutcomeEntry entry);
    }
}

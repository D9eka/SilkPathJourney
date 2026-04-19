using System.Collections.Generic;
using Internal.Scripts.Events.Data;

namespace Internal.Scripts.UI.Screens.HazardQte
{
    public readonly struct HazardResultState
    {
        public readonly bool Success;
        public readonly List<EventOutcomeEntry> Outcomes;

        public HazardResultState(bool success, List<EventOutcomeEntry> outcomes)
        {
            Success = success;
            Outcomes = outcomes;
        }
    }
}

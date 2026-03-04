using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class CompanionApplier : IOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.AddCompanion
        };

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            Debug.LogWarning($"[SPJ Events] AddCompanion НЕ РЕАЛИЗОВАНО (param={entry.Param}, value={entry.Value})");
        }
    }
}

using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.UI.Components;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class CompanionOutcomeHandler : IOutcomeHandler
    {
        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.AddCompanion
        };

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public ResourceType? GetAffectedResource(EventOutcomeType type) => null;

        public void Apply(EventOutcomeEntry entry)
        {
            Debug.LogWarning($"[SPJ Events] AddCompanion НЕ РЕАЛИЗОВАНО (param={entry.Param}, value={entry.Value})");
        }

        public bool CanAfford(EventOutcomeType type, float netValue) => true;
    }
}

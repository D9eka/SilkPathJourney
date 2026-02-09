using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.UI.Components;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class SkillOutcomeHandler : IOutcomeHandler
    {
        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.AddSkillXp
        };

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public ResourceType? GetAffectedResource(EventOutcomeType type) => null;

        public void Apply(EventOutcomeEntry entry)
        {
            Debug.LogWarning($"[SPJ Events] AddSkillXp НЕ РЕАЛИЗОВАНО (param={entry.Param}, value={entry.Value})");
        }

        public bool CanAfford(EventOutcomeType type, float netValue) => true;
    }
}

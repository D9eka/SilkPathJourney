using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class SkipDaysApplier : IOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.SkipDays
        };

        private readonly DayTracker _dayTracker;

        public SkipDaysApplier(DayTracker dayTracker)
        {
            _dayTracker = dayTracker;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            int days = Mathf.RoundToInt(entry.Value);
            if (days <= 0) return;
            _dayTracker.AdvanceDays(days);
        }
    }
}

using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class QueueEventApplier : IOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.QueueEvent
        };

        private readonly EventQueueService _eventQueueService;

        public QueueEventApplier(EventQueueService eventQueueService)
        {
            _eventQueueService = eventQueueService;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Param))
            {
                Debug.LogWarning("[SPJ Events] QueueEvent outcome has empty Param.");
                return;
            }

            int delayDays = Mathf.Max(0, Mathf.RoundToInt(entry.Value));
            _eventQueueService.Enqueue(entry.Param, delayDays);
        }
    }
}

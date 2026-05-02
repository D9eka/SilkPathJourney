using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Quests;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class MarkPendingEndingApplier : IOutcomeApplier
    {
        private readonly QuestPendingEndingsService _pendingEndings;

        public MarkPendingEndingApplier(QuestPendingEndingsService pendingEndings)
        {
            _pendingEndings = pendingEndings;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => new[] { EventOutcomeType.MarkPendingEnding };

        public void Apply(EventOutcomeEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Param))
            {
                Debug.LogWarning("[SPJ Quests] MarkPendingEnding outcome has empty Param.");
                return;
            }

            _pendingEndings.AddPendingEnding(entry.Param);
        }
    }
}

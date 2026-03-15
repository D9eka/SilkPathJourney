using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Quests;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class FailQuestApplier : IOutcomeApplier
    {
        private readonly QuestRepository _questRepository;

        public FailQuestApplier(QuestRepository questRepository)
        {
            _questRepository = questRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => new[] { EventOutcomeType.FailQuest };

        public void Apply(EventOutcomeEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Param))
            {
                Debug.LogWarning("[SPJ Quests] FailQuest outcome has empty Param.");
                return;
            }

            _questRepository.FailQuest(entry.Param);
        }
    }
}

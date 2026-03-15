using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Quests;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class CompleteQuestApplier : IOutcomeApplier
    {
        private readonly QuestRepository _questRepository;

        public CompleteQuestApplier(QuestRepository questRepository)
        {
            _questRepository = questRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => new[] { EventOutcomeType.CompleteQuest };

        public void Apply(EventOutcomeEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Param))
            {
                Debug.LogWarning("[SPJ Quests] CompleteQuest outcome has empty Param.");
                return;
            }

            _questRepository.CompleteQuest(entry.Param);
        }
    }
}

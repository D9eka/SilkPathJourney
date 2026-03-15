using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Quests;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class StartQuestApplier : IOutcomeApplier
    {
        private readonly QuestRepository _questRepository;

        public StartQuestApplier(QuestRepository questRepository)
        {
            _questRepository = questRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => new[] { EventOutcomeType.StartQuest };

        public void Apply(EventOutcomeEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Param))
            {
                Debug.LogWarning("[SPJ Quests] StartQuest outcome has empty Param.");
                return;
            }

            _questRepository.StartQuest(entry.Param);
        }
    }
}

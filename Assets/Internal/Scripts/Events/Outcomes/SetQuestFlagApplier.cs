using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Quests;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class SetQuestFlagApplier : IOutcomeApplier
    {
        private readonly QuestRepository _questRepository;

        public SetQuestFlagApplier(QuestRepository questRepository)
        {
            _questRepository = questRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => new[] { EventOutcomeType.SetQuestFlag };

        public void Apply(EventOutcomeEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Param))
            {
                Debug.LogWarning("[SPJ Quests] SetQuestFlag outcome has empty Param.");
                return;
            }

            int dotIndex = entry.Param.IndexOf('.');
            if (dotIndex < 0)
            {
                Debug.LogWarning($"[SPJ Quests] SetQuestFlag Param '{entry.Param}' must be 'questId.flagId'.");
                return;
            }

            string questId = entry.Param.Substring(0, dotIndex);
            string flagId = entry.Param.Substring(dotIndex + 1);
            _questRepository.IncrementFlag(questId, flagId, (int)entry.Value);
        }
    }
}

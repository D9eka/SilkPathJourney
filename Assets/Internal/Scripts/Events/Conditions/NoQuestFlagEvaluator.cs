using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Quests;

namespace Internal.Scripts.Events.Conditions
{
    public class NoQuestFlagEvaluator : IConditionEvaluator
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.NoQuestFlag
        };

        private readonly QuestRepository _questRepository;

        public NoQuestFlagEvaluator(QuestRepository questRepository)
        {
            _questRepository = questRepository;
        }

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            int dotIndex = condition.Param.IndexOf('.');
            if (dotIndex < 0) return false;

            string questId = condition.Param.Substring(0, dotIndex);
            string flagId = condition.Param.Substring(dotIndex + 1);
            return _questRepository.GetFlag(questId, flagId) < (int)condition.Value;
        }
    }
}

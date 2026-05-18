using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Quests;

namespace Internal.Scripts.Events.Conditions
{
    public class QuestConditionEvaluator : IConditionEvaluator
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.ActiveQuest,
            EventConditionType.NoActiveQuest,
            EventConditionType.CompletedQuest,
            EventConditionType.NotCompletedQuest,
            EventConditionType.QuestAvailable
        };

        private readonly QuestRepository _questRepository;
        private readonly QuestAvailabilityService _availability;

        public QuestConditionEvaluator(QuestRepository questRepository, QuestAvailabilityService availability)
        {
            _questRepository = questRepository;
            _availability = availability;
        }

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            return condition.Type switch
            {
                EventConditionType.ActiveQuest => _questRepository.IsActive(condition.Param),
                EventConditionType.NoActiveQuest => !_questRepository.IsActive(condition.Param),
                EventConditionType.CompletedQuest => _questRepository.IsCompleted(condition.Param),
                EventConditionType.NotCompletedQuest =>
                    !_questRepository.IsCompleted(condition.Param) && !_questRepository.IsFailed(condition.Param),
                EventConditionType.QuestAvailable => _availability.IsAvailable(condition.Param),
                _ => false
            };
        }
    }
}

using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Quests;

namespace Internal.Scripts.Events.Conditions
{
    public class QuestStageEvaluator : IConditionEvaluator
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.QuestStage
        };

        private readonly QuestRepository _questRepository;

        public QuestStageEvaluator(QuestRepository questRepository)
        {
            _questRepository = questRepository;
        }

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            return _questRepository.GetCurrentStageIndex(condition.Param) >= (int)condition.Value;
        }
    }
}

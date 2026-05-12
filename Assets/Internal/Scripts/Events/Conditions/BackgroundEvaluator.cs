using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Save;

namespace Internal.Scripts.Events.Conditions
{
    public class BackgroundEvaluator : IConditionEvaluator
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.HasBackground
        };

        private readonly SaveRepository _saveRepository;

        public BackgroundEvaluator(SaveRepository saveRepository)
        {
            _saveRepository = saveRepository;
        }

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            return _saveRepository.Data.SelectedBackgroundId == condition.Param;
        }
    }
}

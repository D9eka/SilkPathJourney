using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Player;

namespace Internal.Scripts.Events.Conditions
{
    public class InCampEvaluator : IConditionEvaluator
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.InCamp
        };

        private readonly CaravanSpeedService _speedService;

        public InCampEvaluator(CaravanSpeedService speedService)
        {
            _speedService = speedService;
        }

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            return _speedService.CurrentMode.CurrentValue == CaravanSpeedMode.Camp;
        }
    }
}

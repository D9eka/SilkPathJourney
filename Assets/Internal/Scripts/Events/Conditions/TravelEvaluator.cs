using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Player;

namespace Internal.Scripts.Events.Conditions
{
    public class TravelEvaluator : IConditionEvaluator
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.InTravel
        };

        private readonly PlayerController _playerController;

        public TravelEvaluator(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            return _playerController.State == PlayerState.Moving;
        }
    }
}

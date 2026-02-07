using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Player;
using Internal.Scripts.Road.Nodes;

namespace Internal.Scripts.Events.Conditions
{
    public class LocationConditionHandler : IConditionHandler
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.NearNode
        };

        private readonly IRoadNodeLookup _nodeLookup;
        private readonly PlayerController _playerController;

        public LocationConditionHandler(IRoadNodeLookup nodeLookup, PlayerController playerController)
        {
            _nodeLookup = nodeLookup;
            _playerController = playerController;
        }

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            string nearestNodeId = _nodeLookup.FindNearestNodeId(_playerController.CurrentPosition);
            return nearestNodeId == condition.Param;
        }
    }
}

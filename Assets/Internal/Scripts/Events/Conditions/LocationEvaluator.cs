using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Player;
using Internal.Scripts.Road.Nodes;

namespace Internal.Scripts.Events.Conditions
{
    public class LocationEvaluator : IConditionEvaluator
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.NearNode
        };

        private readonly IRoadNodeLookup _nodeLookup;
        private readonly PlayerController _playerController;

        public LocationEvaluator(IRoadNodeLookup nodeLookup, PlayerController playerController)
        {
            _nodeLookup = nodeLookup;
            _playerController = playerController;
        }

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            string nearestNodeId = _nodeLookup.FindNearestNodeId(_playerController.CurrentPosition);

            if (condition.ParamList != null && condition.ParamList.Length > 0)
            {
                foreach (string id in condition.ParamList)
                {
                    if (string.Equals(nearestNodeId, id, System.StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }

            if (string.IsNullOrEmpty(condition.Param)) return false;
            return string.Equals(nearestNodeId, condition.Param, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}

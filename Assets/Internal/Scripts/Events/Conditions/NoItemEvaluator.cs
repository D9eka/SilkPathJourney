using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Inventory;

namespace Internal.Scripts.Events.Conditions
{
    public class NoItemEvaluator : IConditionEvaluator
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.NoItem
        };

        private readonly InventoryRepository _inventoryRepository;

        public NoItemEvaluator(InventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            return !_inventoryRepository.HasPlayerItem(condition.Param, 1);
        }
    }
}

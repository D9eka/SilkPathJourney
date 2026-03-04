using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Inventory;
using UnityEngine;

namespace Internal.Scripts.Events.Conditions
{
    public class InventoryEvaluator : IConditionEvaluator
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.HasItem
        };

        private readonly InventoryRepository _inventoryRepository;

        public InventoryEvaluator(InventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            return _inventoryRepository.HasPlayerItem(condition.Param, Mathf.RoundToInt(condition.Value));
        }
    }
}

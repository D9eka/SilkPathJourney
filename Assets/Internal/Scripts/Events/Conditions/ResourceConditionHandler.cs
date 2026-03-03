using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;

namespace Internal.Scripts.Events.Conditions
{
    public class ResourceConditionHandler : IConditionHandler
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.MinMoney, EventConditionType.MaxMoney,
            EventConditionType.MinFood, EventConditionType.MaxFood,
            EventConditionType.MinDanger, EventConditionType.MaxDanger
        };

        private static readonly Dictionary<EventConditionType, Func<PlayerResourceState, float>> Getters = new()
        {
            [EventConditionType.MinMoney] = r => r.Money,
            [EventConditionType.MaxMoney] = r => r.Money,
            [EventConditionType.MinDanger] = r => r.AccumulatedDanger,
            [EventConditionType.MaxDanger] = r => r.AccumulatedDanger
        };

        private static readonly HashSet<EventConditionType> MinTypes = new()
        {
            EventConditionType.MinMoney,
            EventConditionType.MinFood,
            EventConditionType.MinDanger
        };

        private readonly InventoryRepository _inventoryRepository;

        public ResourceConditionHandler(InventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            float current;

            if (condition.Type == EventConditionType.MinFood || condition.Type == EventConditionType.MaxFood)
            {
                current = InventoryStateMutator.GetItemCount(
                    _inventoryRepository.GetPlayerInventory(), SuppliesItemId.Value);
            }
            else
            {
                if (!Getters.TryGetValue(condition.Type, out var getter))
                    return true;

                current = getter(resources);
            }

            return MinTypes.Contains(condition.Type)
                ? current >= condition.Value
                : current <= condition.Value;
        }

    }
}

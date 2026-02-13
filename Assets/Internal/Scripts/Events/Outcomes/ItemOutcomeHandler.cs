using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Inventory;
using Internal.Scripts.UI.Components;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class ItemOutcomeHandler : IOutcomeHandler
    {
        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.AddItem
        };

        private readonly InventoryRepository _inventoryRepository;

        public ItemOutcomeHandler(InventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public ResourceType? GetAffectedResource(EventOutcomeType type) => null;

        public void Apply(EventOutcomeEntry entry)
        {
            int count = Mathf.RoundToInt(entry.Value);
            if (count > 0)
            {
                _inventoryRepository.UpdatePlayerInventory(state =>
                    InventoryStateMutator.AddItems(state, entry.Param, count));
            }
            else if (count < 0)
            {
                _inventoryRepository.UpdatePlayerInventory(state =>
                    InventoryStateMutator.RemoveItems(state, entry.Param, -count));
            }
        }

        public bool CanAfford(EventOutcomeType type, float netValue) => true;
    }
}

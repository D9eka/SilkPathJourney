using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Inventory;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class RemoveItemApplier : IOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types = { EventOutcomeType.RemoveItem };

        private readonly InventoryRepository _inventoryRepository;

        public RemoveItemApplier(InventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            int count = Mathf.RoundToInt(entry.Value);
            if (count <= 0) return;

            _inventoryRepository.UpdatePlayerInventory(state =>
                InventoryStateMutator.RemoveItems(state, entry.Param, count));
        }
    }
}

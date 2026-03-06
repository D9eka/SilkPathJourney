using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.UI.Components;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class ResourceApplier : IResourceOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.Money,
            EventOutcomeType.Food,
            EventOutcomeType.Danger
        };

        private readonly PlayerResourceRepository _resourceRepository;
        private readonly InventoryRepository _inventoryRepository;
        private readonly GameBalanceConfig _balanceConfig;

        public ResourceApplier(PlayerResourceRepository resourceRepository,
            InventoryRepository inventoryRepository,
            GameBalanceConfig balanceConfig)
        {
            _resourceRepository = resourceRepository;
            _inventoryRepository = inventoryRepository;
            _balanceConfig = balanceConfig;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public ResourceType? GetAffectedResource(EventOutcomeType type) => type switch
        {
            EventOutcomeType.Money => ResourceType.Money,
            EventOutcomeType.Food => ResourceType.Food,
            EventOutcomeType.Danger => ResourceType.Danger,
            _ => null
        };

        public void Apply(EventOutcomeEntry entry)
        {
            switch (entry.Type)
            {
                case EventOutcomeType.Money:
                    _resourceRepository.UpdateResources(s => s.Money += Mathf.RoundToInt(entry.Value));
                    break;
                case EventOutcomeType.Food:
                    int foodDelta = Mathf.RoundToInt(entry.Value);
                    if (foodDelta > 0)
                        _inventoryRepository.UpdatePlayerInventory(inv =>
                            InventoryStateMutator.AddItems(inv, SuppliesItemId.Value, foodDelta));
                    else if (foodDelta < 0)
                        _inventoryRepository.UpdatePlayerInventory(inv =>
                            InventoryStateMutator.RemoveItems(inv, SuppliesItemId.Value, -foodDelta));
                    break;
                case EventOutcomeType.Danger:
                    _resourceRepository.UpdateResources(s =>
                        s.AccumulatedDanger = Mathf.Max(0f, s.AccumulatedDanger + entry.Value));
                    break;
            }
        }

        public bool CanAfford(EventOutcomeType type, float netValue)
        {
            if (netValue >= 0) return true;
            var res = _resourceRepository.Current;
            if (res == null) return true;
            return type switch
            {
                EventOutcomeType.Money => res.Money + netValue >= 0,
                EventOutcomeType.Food => true,
                _ => true
            };
        }
    }
}

using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Items;
using R3;

namespace Internal.Scripts.Inventory
{
    public sealed class InventoryModel
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly ItemRowsBuilder _rowsBuilder;
        private readonly ItemWeightCalculator _weightCalculator;
        private readonly ReactiveProperty<InventoryViewState> _state;
        private IDisposable _subscription;
        private int _lastItemsHash;
        private IReadOnlyList<ItemRowData> _lastItems = Array.Empty<ItemRowData>();

        public InventoryModel(InventoryRepository inventoryRepository, EconomyDatabase economyDatabase)
        {
            _inventoryRepository = inventoryRepository;
            ItemCatalog itemCatalog = new ItemCatalog(economyDatabase);
            _rowsBuilder = new ItemRowsBuilder(itemCatalog);
            _weightCalculator = new ItemWeightCalculator(itemCatalog);
            _state = new ReactiveProperty<InventoryViewState>(
                new InventoryViewState(Array.Empty<ItemRowData>(), 0, 0, 0f, 0f));
        }

        public Observable<InventoryViewState> State => _state;

        public void Activate()
        {
            if (_subscription != null)
                return;

            _subscription = _inventoryRepository.PlayerInventoryStream.Subscribe(HandleInventory);
            HandleInventory(_inventoryRepository.GetPlayerInventory());
        }

        public void Deactivate()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        public void DropItem(string itemId, int count)
        {
            if (string.IsNullOrWhiteSpace(itemId) || count <= 0)
                return;

            _inventoryRepository.UpdatePlayerInventory(state => InventoryStateMutator.RemoveItems(state, itemId, count));
        }

        private void HandleInventory(InventoryState inventory)
        {
            if (inventory == null)
                return;

            int itemsHash = _rowsBuilder.ComputeItemsHash(inventory);
            IReadOnlyList<ItemRowData> items = _lastItems;
            if (itemsHash != _lastItemsHash)
            {
                items = _rowsBuilder.BuildRows(inventory, includePrice: true);
                _lastItems = items;
                _lastItemsHash = itemsHash;
            }

            float weight = _weightCalculator.CalculateInventoryWeight(inventory);
            _state.Value = new InventoryViewState(items, _lastItemsHash, inventory.Money, weight, inventory.MaxWeightKg);
        }
    }
}

using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Items;
using R3;

namespace Internal.Scripts.Inventory
{
    public sealed class InventoryModel
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly ItemRowsBuilder _rowsBuilder;
        private readonly ItemWeightCalculator _weightCalculator;
        private readonly ReactiveProperty<InventoryViewState> _state;
        private IDisposable _inventorySubscription;
        private IDisposable _resourceSubscription;
        private int _lastItemsHash;
        private IReadOnlyList<ItemRowData> _lastItems = Array.Empty<ItemRowData>();
        private InventoryState _currentInventory;
        private PlayerResourceState _currentResources;

        public InventoryModel(
            InventoryRepository inventoryRepository,
            PlayerResourceRepository resourceRepository,
            EconomyDatabase economyDatabase)
        {
            _inventoryRepository = inventoryRepository;
            _resourceRepository = resourceRepository;
            ItemCatalog itemCatalog = new ItemCatalog(economyDatabase);
            _rowsBuilder = new ItemRowsBuilder(itemCatalog);
            _weightCalculator = new ItemWeightCalculator(itemCatalog);
            _state = new ReactiveProperty<InventoryViewState>(
                new InventoryViewState(Array.Empty<ItemRowData>(), 0, 0, 0f, 0f));
        }

        public Observable<InventoryViewState> State => _state;

        public void Activate()
        {
            if (_inventorySubscription != null)
                return;

            _inventorySubscription = _inventoryRepository.PlayerInventoryStream.Subscribe(HandleInventory);
            _resourceSubscription = _resourceRepository.StateStream.Subscribe(HandleResources);

            _currentInventory = _inventoryRepository.GetPlayerInventory();
            _currentResources = _resourceRepository.Current;
            RebuildState();
        }

        public void Deactivate()
        {
            _inventorySubscription?.Dispose();
            _inventorySubscription = null;
            _resourceSubscription?.Dispose();
            _resourceSubscription = null;
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

            _currentInventory = inventory;
            RebuildState();
        }

        private void HandleResources(PlayerResourceState resources)
        {
            if (resources == null)
                return;

            _currentResources = resources;
            RebuildState();
        }

        private void RebuildState()
        {
            if (_currentInventory == null || _currentResources == null)
                return;

            int itemsHash = _rowsBuilder.ComputeItemsHash(_currentInventory);
            IReadOnlyList<ItemRowData> items = _lastItems;
            if (itemsHash != _lastItemsHash)
            {
                items = _rowsBuilder.BuildRows(_currentInventory, includePrice: true);
                _lastItems = items;
                _lastItemsHash = itemsHash;
            }

            float weight = _weightCalculator.CalculateInventoryWeight(_currentInventory);
            int money = _currentResources.Money;
            float maxWeight = _currentResources.TotalCapacity;

            _state.Value = new InventoryViewState(items, _lastItemsHash, money, weight, maxWeight);
        }
    }
}

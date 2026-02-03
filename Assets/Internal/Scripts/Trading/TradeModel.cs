using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using R3;
using UnityEngine;

namespace Internal.Scripts.Trading
{
    public sealed class TradeModel
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly TradeSession _session = new();
        private readonly TradeCityCatalog _cityCatalog;
        private readonly ItemRowsBuilder _rowsBuilder;
        private readonly TradeTotalsCalculator _totalsCalculator;
        private readonly ItemWeightCalculator _weightCalculator;
        private readonly ReactiveProperty<TradeViewState> _state;

        private IDisposable _playerSubscription;
        private IDisposable _citySubscription;
        private string _cityId;
        private InventoryState _playerInventory;
        private InventoryState _npcInventory;
        private int _playerItemsHash;
        private int _npcItemsHash;
        private int _buyItemsHash;
        private int _sellItemsHash;
        private IReadOnlyList<ItemRowData> _playerItems = Array.Empty<ItemRowData>();
        private IReadOnlyList<ItemRowData> _npcItems = Array.Empty<ItemRowData>();
        private IReadOnlyList<ItemRowData> _buyItems = Array.Empty<ItemRowData>();
        private IReadOnlyList<ItemRowData> _sellItems = Array.Empty<ItemRowData>();

        public TradeModel(InventoryRepository inventoryRepository, EconomyDatabase economyDatabase)
        {
            _inventoryRepository = inventoryRepository;
            ItemCatalog itemCatalog = new ItemCatalog(economyDatabase);
            _cityCatalog = new TradeCityCatalog(economyDatabase);
            _rowsBuilder = new ItemRowsBuilder(itemCatalog);
            _totalsCalculator = new TradeTotalsCalculator(itemCatalog);
            _weightCalculator = new ItemWeightCalculator(itemCatalog);
            _state = new ReactiveProperty<TradeViewState>(
                new TradeViewState(false, true,
                    true, string.Empty));
        }

        public Observable<TradeViewState> State => _state;

        public void Activate(string cityId)
        {
            Deactivate();
            _cityId = cityId;
            _session.Clear();
            ResetCaches();

            _playerSubscription = _inventoryRepository.PlayerInventoryStream.Subscribe(HandlePlayerInventory);
            if (!string.IsNullOrWhiteSpace(_cityId))
                _citySubscription = _inventoryRepository.ObserveCityInventory(_cityId).Subscribe(HandleCityInventory);

            HandlePlayerInventory(_inventoryRepository.GetPlayerInventory());
            if (!string.IsNullOrWhiteSpace(_cityId))
            {
                CityInventoryState cityInventory = _inventoryRepository.GetCityInventory(_cityId);
                HandleCityInventory(cityInventory != null ? cityInventory.Inventory : new InventoryState());
            }
            else
            {
                HandleCityInventory(new InventoryState());
            }
        }

        public void Deactivate()
        {
            _playerSubscription?.Dispose();
            _playerSubscription = null;
            _citySubscription?.Dispose();
            _citySubscription = null;
            _cityId = null;
            _playerInventory = null;
            _npcInventory = null;
            _session.Clear();
            ResetCaches();
        }

        public void MoveToBuy(string itemId, bool addAll)
        {
            if (_session.MoveToBuy(itemId, addAll))
                RebuildState();
        }

        public void MoveToSell(string itemId, bool addAll)
        {
            if (_session.MoveToSell(itemId, addAll))
                RebuildState();
        }

        public void ReturnFromBuy(string itemId, bool addAll)
        {
            if (_session.ReturnFromBuy(itemId, addAll))
                RebuildState();
        }

        public void ReturnFromSell(string itemId, bool addAll)
        {
            if (_session.ReturnFromSell(itemId, addAll))
                RebuildState();
        }

        public void ExecuteTrade()
        {
            if (_playerInventory == null || string.IsNullOrWhiteSpace(_cityId))
                return;

            CityInventoryState citySnapshot = _inventoryRepository.GetCityInventory(_cityId);
            InventoryState npcSnapshot = citySnapshot != null ? citySnapshot.Inventory : null;
            if (npcSnapshot == null) return;

            int buyTotal = _totalsCalculator.CalculateTotal(_session.ToBuy);
            int sellTotal = _totalsCalculator.CalculateTotal(_session.ToSell);

            if (!_totalsCalculator.HasPlayerFunds(buyTotal, sellTotal, _playerInventory.Money))
                return;

            int npcFundsAvailable = Mathf.Max(0, npcSnapshot.Money + buyTotal);
            int paymentToPlayer = Mathf.Min(npcFundsAvailable, sellTotal);

            int playerMoney = _playerInventory.Money - buyTotal + paymentToPlayer;
            int npcMoney = npcSnapshot.Money + buyTotal - paymentToPlayer;

            Dictionary<string, int> toBuy = new(_session.ToBuy);
            Dictionary<string, int> toSell = new(_session.ToSell);
            _session.ClearToDictionaries();

            _inventoryRepository.UpdatePlayerInventory(state =>
            {
                ApplyTradeToPlayer(state, toBuy, toSell);
                state.Money = Mathf.Max(0, playerMoney);
            });

            _inventoryRepository.UpdateCityInventory(_cityId, state =>
            {
                ApplyTradeToNpc(state, toBuy, toSell);
                state.Money = Mathf.Max(0, npcMoney);
            });

            RebuildState();
        }

        private void HandlePlayerInventory(InventoryState inventory)
        {
            if (inventory == null) return;

            _playerInventory = inventory;
            _session.SetPlayerBase(inventory);
            RebuildState();
        }

        private void HandleCityInventory(InventoryState inventory)
        {
            if (inventory == null) return;

            _npcInventory = inventory;
            _session.SetNpcBase(inventory);
            RebuildState();
        }

        private void RebuildState()
        {
            _session.ClampReserved();
            UpdateItemsIfChanged();

            int buyTotal = _totalsCalculator.CalculateTotal(_session.ToBuy);
            int sellTotal = _totalsCalculator.CalculateTotal(_session.ToSell);

            int playerMoney = _playerInventory != null ? _playerInventory.Money : 0;
            int npcMoney = _npcInventory != null ? _npcInventory.Money : 0;

            bool playerEnough = _playerInventory != null && _totalsCalculator.HasPlayerFunds(buyTotal, sellTotal, playerMoney);
            bool npcEnough = _npcInventory == null || _totalsCalculator.HasNpcFunds(buyTotal, sellTotal, npcMoney);

            float baseWeight = _playerInventory != null ? _weightCalculator.CalculateInventoryWeight(_playerInventory) : 0f;
            float buyWeight = _weightCalculator.CalculateWeightDelta(_session.ToBuy);
            float sellWeight = _weightCalculator.CalculateWeightDelta(_session.ToSell);
            float projected = Mathf.Max(0f, baseWeight + buyWeight - sellWeight);
            float maxWeight = _playerInventory != null ? _playerInventory.MaxWeightKg : 0f;
            bool warning = maxWeight > 0f && projected > maxWeight;

            string npcName = _cityCatalog.ResolveCityName(_cityId);

            _state.Value = new TradeViewState(_playerItems, _npcItems, _buyItems, _sellItems, _playerItemsHash,
                _npcItemsHash, _buyItemsHash, _sellItemsHash, playerMoney, npcMoney, buyTotal, sellTotal, projected,
                maxWeight, warning, playerEnough, npcEnough, npcName);
        }

        private void UpdateItemsIfChanged()
        {
            int playerHash = _rowsBuilder.ComputeRemainingHash(_session.PlayerBase, _session.ToSell);
            if (playerHash != _playerItemsHash)
            {
                _playerItems = _rowsBuilder.BuildRemainingRows(_session.PlayerBase, _session.ToSell, includePrice: true);
                _playerItemsHash = playerHash;
            }

            int npcHash = _rowsBuilder.ComputeRemainingHash(_session.NpcBase, _session.ToBuy);
            if (npcHash != _npcItemsHash)
            {
                _npcItems = _rowsBuilder.BuildRemainingRows(_session.NpcBase, _session.ToBuy, includePrice: true);
                _npcItemsHash = npcHash;
            }

            int buyHash = _rowsBuilder.ComputeCountsHash(_session.ToBuy);
            if (buyHash != _buyItemsHash)
            {
                _buyItems = _rowsBuilder.BuildRows(_session.ToBuy, includePrice: true);
                _buyItemsHash = buyHash;
            }

            int sellHash = _rowsBuilder.ComputeCountsHash(_session.ToSell);
            if (sellHash != _sellItemsHash)
            {
                _sellItems = _rowsBuilder.BuildRows(_session.ToSell, includePrice: true);
                _sellItemsHash = sellHash;
            }
        }

        private static void ApplyTradeToPlayer(InventoryState player, Dictionary<string, int> toBuy, Dictionary<string, int> toSell)
        {
            if (player == null)
                return;

            foreach (KeyValuePair<string, int> kvp in toBuy)
                InventoryStateMutator.AddItems(player, kvp.Key, kvp.Value);

            foreach (KeyValuePair<string, int> kvp in toSell)
                InventoryStateMutator.RemoveItems(player, kvp.Key, kvp.Value);
        }

        private static void ApplyTradeToNpc(InventoryState npc, Dictionary<string, int> toBuy, Dictionary<string, int> toSell)
        {
            if (npc == null)
                return;

            foreach (KeyValuePair<string, int> kvp in toBuy)
                InventoryStateMutator.RemoveItems(npc, kvp.Key, kvp.Value);

            foreach (KeyValuePair<string, int> kvp in toSell)
                InventoryStateMutator.AddItems(npc, kvp.Key, kvp.Value);
        }

        private void ResetCaches()
        {
            _playerItemsHash = 0;
            _npcItemsHash = 0;
            _buyItemsHash = 0;
            _sellItemsHash = 0;
            _playerItems = Array.Empty<ItemRowData>();
            _npcItems = Array.Empty<ItemRowData>();
            _buyItems = Array.Empty<ItemRowData>();
            _sellItems = Array.Empty<ItemRowData>();
        }
    }
}

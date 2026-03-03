using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Encounter;
using R3;
using UnityEngine;

namespace Internal.Scripts.Trading
{
    public sealed class TradeModel
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly CityTradePriceService _priceService;
        private readonly CityTransactionService _cityTransactionService;
        private readonly TradeSession _session = new();
        private readonly TradeCityCatalog _cityCatalog;
        private readonly ItemRowsBuilder _rowsBuilder;
        private readonly TradeTotalsCalculator _totalsCalculator;
        private readonly ItemWeightCalculator _weightCalculator;
        private readonly ItemCatalog _itemCatalog;
        private readonly ReactiveProperty<TradeViewState> _state;

        private IDisposable _playerSubscription;
        private IDisposable _citySubscription;
        private IDisposable _resourceSubscription;
        private string _cityId;
        private InventoryState _playerInventory;
        private InventoryState _npcInventory;
        private PlayerResourceState _playerResources;
        private bool _isNpcTrade;
        private InventoryState _npcLocalInventory;
        private string _npcDisplayName;
        private ITradePricing _pricing;
        private int _playerItemsHash;
        private int _npcItemsHash;
        private int _buyItemsHash;
        private int _sellItemsHash;
        private IReadOnlyList<ItemRowData> _playerItems = Array.Empty<ItemRowData>();
        private IReadOnlyList<ItemRowData> _npcItems = Array.Empty<ItemRowData>();
        private IReadOnlyList<ItemRowData> _buyItems = Array.Empty<ItemRowData>();
        private IReadOnlyList<ItemRowData> _sellItems = Array.Empty<ItemRowData>();

        public TradeModel(
            InventoryRepository inventoryRepository,
            PlayerResourceRepository resourceRepository,
            EconomyDatabase economyDatabase,
            CityTradePriceService priceService,
            CityTransactionService cityTransactionService)
        {
            _inventoryRepository = inventoryRepository;
            _resourceRepository = resourceRepository;
            _priceService = priceService;
            _cityTransactionService = cityTransactionService;
            _itemCatalog = new ItemCatalog(economyDatabase);
            _cityCatalog = new TradeCityCatalog(economyDatabase);
            _rowsBuilder = new ItemRowsBuilder(_itemCatalog);
            _totalsCalculator = new TradeTotalsCalculator(_itemCatalog);
            _weightCalculator = new ItemWeightCalculator(_itemCatalog);
            _state = new ReactiveProperty<TradeViewState>(
                new TradeViewState(false, true,
                    true, string.Empty));
        }

        public Observable<TradeViewState> State => _state;

        public void Activate(string cityId)
        {
            Deactivate();
            _cityId = cityId;
            _pricing = new CityTradePricing(_priceService, cityId);
            ActivateCore();

            if (!string.IsNullOrWhiteSpace(_cityId))
                _citySubscription = _inventoryRepository.ObserveCityInventory(_cityId).Subscribe(HandleCityInventory);

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

        public void ActivateWithNpc(NpcTradeArgs args)
        {
            Deactivate();
            _isNpcTrade = true;
            _pricing = new NpcTradePricing(_itemCatalog, args.Markup, args.SuppliesMarkupMultiplier);
            _npcDisplayName = args.Agent.EconomyState.Name;

            InventoryState srcInv = args.Agent.EconomyState.Inventory;
            _npcLocalInventory = new InventoryState { Money = args.Agent.EconomyState.Money };
            if (srcInv?.Items != null)
            {
                foreach (ItemStackState stack in srcInv.Items)
                    _npcLocalInventory.Items.Add(new ItemStackState { ItemId = stack.ItemId, Count = stack.Count });
            }

            ActivateCore();
            HandleCityInventory(_npcLocalInventory);
        }

        private void ActivateCore()
        {
            _session.Clear();
            ResetCaches();
            _playerSubscription = _inventoryRepository.PlayerInventoryStream.Subscribe(HandlePlayerInventory);
            _resourceSubscription = _resourceRepository.StateStream.Subscribe(HandlePlayerResources);
            HandlePlayerInventory(_inventoryRepository.GetPlayerInventory());
            _playerResources = _resourceRepository.Current;
        }

        public void Deactivate()
        {
            _playerSubscription?.Dispose();
            _playerSubscription = null;
            _citySubscription?.Dispose();
            _citySubscription = null;
            _resourceSubscription?.Dispose();
            _resourceSubscription = null;
            _cityId = null;
            _playerInventory = null;
            _npcInventory = null;
            _playerResources = null;
            _isNpcTrade = false;
            _pricing = null;
            _npcLocalInventory = null;
            _npcDisplayName = null;
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
            ExecuteTradeCore();
        }

        private void ExecuteTradeCore()
        {
            if (!ValidateTrade(out int npcCurrentMoney))
                return;

            int buyTotal = _totalsCalculator.CalculateTotal(_session.ToBuy, _pricing.GetBuyPrice);
            int sellTotal = _totalsCalculator.CalculateTotal(_session.ToSell, _pricing.GetSellPrice);

            if (!_totalsCalculator.HasPlayerFunds(buyTotal, sellTotal, _playerResources.Money))
                return;

            (int playerMoney, int npcMoney) = CalculateMoneyFlows(
                buyTotal, sellTotal, npcCurrentMoney);

            Dictionary<string, int> toBuy = new(_session.ToBuy);
            Dictionary<string, int> toSell = new(_session.ToSell);
            _session.ClearToDictionaries();

            ApplyTradeResults(toBuy, toSell, playerMoney, npcMoney);
            RebuildState();
        }

        private bool ValidateTrade(out int npcMoney)
        {
            npcMoney = 0;

            if (_playerInventory == null || _playerResources == null)
                return false;

            if (_isNpcTrade)
            {
                if (_npcLocalInventory == null) return false;
                npcMoney = _npcLocalInventory.Money;
                return true;
            }

            if (string.IsNullOrWhiteSpace(_cityId)) return false;
            CityInventoryState citySnapshot = _inventoryRepository.GetCityInventory(_cityId);
            InventoryState npcSnapshot = citySnapshot != null ? citySnapshot.Inventory : null;
            if (npcSnapshot == null) return false;
            npcMoney = npcSnapshot.Money;
            return true;
        }

        private (int playerMoney, int npcMoney) CalculateMoneyFlows(
            int buyTotal, int sellTotal, int npcCurrentMoney)
        {
            int npcFundsAvailable = Mathf.Max(0, npcCurrentMoney + buyTotal);
            int paymentToPlayer = Mathf.Min(npcFundsAvailable, sellTotal);

            int playerMoney = _playerResources.Money - buyTotal + paymentToPlayer;
            int npcMoney = npcCurrentMoney + buyTotal - paymentToPlayer;
            return (playerMoney, npcMoney);
        }

        private void ApplyTradeResults(
            Dictionary<string, int> toBuy, Dictionary<string, int> toSell,
            int playerMoney, int npcMoney)
        {
            _inventoryRepository.UpdatePlayerInventory(state =>
            {
                InventoryStateMutator.ApplyTrade(state, toBuy, toSell);
            });

            _resourceRepository.UpdateResources(state =>
            {
                state.Money = Mathf.Max(0, playerMoney);
            });

            if (_isNpcTrade)
            {
                InventoryStateMutator.ApplyTrade(_npcLocalInventory, toSell, toBuy);
                _npcLocalInventory.Money = Mathf.Max(0, npcMoney);
                HandleCityInventory(_npcLocalInventory);
            }
            else
            {
                _cityTransactionService.ApplyBatchTrade(_cityId, toBuy, toSell, npcMoney);
            }
        }

        public (InventoryState inventory, int money)? GetNpcTradeResult()
        {
            if (!_isNpcTrade || _npcLocalInventory == null)
                return null;
            return (_npcLocalInventory, _npcLocalInventory.Money);
        }

        private void HandlePlayerInventory(InventoryState inventory)
        {
            if (inventory == null) return;

            _playerInventory = inventory;
            _session.SetPlayerBase(inventory);
            RebuildState();
        }

        private void HandlePlayerResources(PlayerResourceState resources)
        {
            if (resources == null) return;

            _playerResources = resources;
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

            int buyTotal = _totalsCalculator.CalculateTotal(_session.ToBuy, _pricing.GetBuyPrice);
            int sellTotal = _totalsCalculator.CalculateTotal(_session.ToSell, _pricing.GetSellPrice);

            int playerMoney = _playerResources != null ? _playerResources.Money : 0;
            int npcMoney = _npcInventory != null ? _npcInventory.Money : 0;

            bool playerEnough = _playerResources != null && _totalsCalculator.HasPlayerFunds(buyTotal, sellTotal, playerMoney);
            bool npcEnough = _npcInventory == null || _totalsCalculator.HasNpcFunds(buyTotal, sellTotal, npcMoney);

            float baseWeight = _playerInventory != null ? _weightCalculator.CalculateInventoryWeight(_playerInventory) : 0f;
            float buyWeight = _weightCalculator.CalculateWeightDelta(_session.ToBuy);
            float sellWeight = _weightCalculator.CalculateWeightDelta(_session.ToSell);
            float projected = Mathf.Max(0f, baseWeight + buyWeight - sellWeight);
            float maxWeight = _playerResources != null ? _playerResources.TotalCapacity : 0f;
            bool warning = maxWeight > 0f && projected > maxWeight;

            string npcName = _isNpcTrade ? _npcDisplayName : _cityCatalog.ResolveCityName(_cityId);

            _state.Value = new TradeViewState(_playerItems, _npcItems, _buyItems, _sellItems, _playerItemsHash,
                _npcItemsHash, _buyItemsHash, _sellItemsHash, playerMoney, npcMoney, buyTotal, sellTotal,
                baseWeight, projected, maxWeight, warning, playerEnough, npcEnough, npcName);
        }

        private void UpdateItemsIfChanged()
        {
            int playerHash = _rowsBuilder.ComputeRemainingHash(_session.PlayerBase, _session.ToSell);
            if (playerHash != _playerItemsHash)
            {
                _playerItems = _rowsBuilder.BuildRemainingRows(_session.PlayerBase, _session.ToSell, _pricing.GetSellPrice);
                _playerItemsHash = playerHash;
            }

            int npcHash = _rowsBuilder.ComputeRemainingHash(_session.NpcBase, _session.ToBuy);
            if (npcHash != _npcItemsHash)
            {
                _npcItems = _rowsBuilder.BuildRemainingRows(_session.NpcBase, _session.ToBuy, _pricing.GetBuyPrice);
                _npcItemsHash = npcHash;
            }

            int buyHash = _rowsBuilder.ComputeCountsHash(_session.ToBuy);
            if (buyHash != _buyItemsHash)
            {
                _buyItems = _rowsBuilder.BuildRows(_session.ToBuy, _pricing.GetBuyPrice);
                _buyItemsHash = buyHash;
            }

            int sellHash = _rowsBuilder.ComputeCountsHash(_session.ToSell);
            if (sellHash != _sellItemsHash)
            {
                _sellItems = _rowsBuilder.BuildRows(_session.ToSell, _pricing.GetSellPrice);
                _sellItemsHash = sellHash;
            }
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

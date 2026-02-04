using System;
using System.Collections.Generic;
using Internal.Scripts.Items;
using Internal.Scripts.Trading;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screen.ViewModel;
using Internal.Scripts.UI.Screens.Inventory;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Trade
{
    public class TradeScreen : PopupScreen
    {
        private enum TradeArea
        {
            Player,
            Npc,
            Buy,
            Sell
        }

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _tradeButtonText;
        [Header("Buttons")]
        [SerializeField] private Button _tradeButton;
        [Header("Content")]
        [SerializeField] private TradeContainer _playerTradeContainer;
        [SerializeField] private TradeContainer _itemsToBuyTradeContainer;
        [SerializeField] private TradeContainer _itemsToSellTradeContainer;
        [SerializeField] private TradeContainer _npcTradeContainer;
        [Header("LocalizedStrings")]
        [SerializeField] private LocalizedString _tradeButtonLocalizedString;

        public TradeContainer PlayerContainer => _playerTradeContainer;
        public TradeContainer NpcContainer => _npcTradeContainer;
        public TradeContainer ItemsToBuyContainer => _itemsToBuyTradeContainer;
        public TradeContainer ItemsToSellContainer => _itemsToSellTradeContainer;

        private TradeScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private TradeArea _activeArea = TradeArea.Player;
        private int _lastPlayerItemsHash;
        private int _lastNpcItemsHash;
        private int _lastBuyItemsHash;
        private int _lastSellItemsHash;
        private bool _hasPlayerItemsHash;
        private bool _hasNpcItemsHash;
        private bool _hasBuyItemsHash;
        private bool _hasSellItemsHash;

        private ItemsView _playerItemsView;
        private ItemsView _npcItemsView;
        private ItemsView _buyItemsView;
        private ItemsView _sellItemsView;

        private Action<ItemRowData> _playerSelectHandler;
        private Action<ItemRowData, bool> _playerActivateHandler;
        private Action<ItemRowData> _npcSelectHandler;
        private Action<ItemRowData, bool> _npcActivateHandler;
        private Action<ItemRowData> _buySelectHandler;
        private Action<ItemRowData, bool> _buyActivateHandler;
        private Action<ItemRowData> _sellSelectHandler;
        private Action<ItemRowData, bool> _sellActivateHandler;

        private UnityAction _tradeHandler;
        private LocalizationHelper.LocalizedTextHandle _tradeButtonHandle;

        protected override void OnEnable()
        {
            base.OnEnable();
            CacheViews();
            BindLocalization();
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            UnsubscribeViewModel();
            base.OnDisable();
            _tradeButtonHandle?.Dispose();
            _tradeButtonHandle = null;
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as TradeScreenViewModel;
            SubscribeViewModel();
        }

        public void SetPlayerItems(IReadOnlyList<ItemRowData> items, bool showWeight, bool showPrice)
        {
            _playerTradeContainer.SetItems(items, showWeight, showPrice);
        }

        public void SetNpcItems(IReadOnlyList<ItemRowData> items, bool showWeight, bool showPrice)
        {
            _npcTradeContainer.SetItems(items, showWeight, showPrice);
        }

        public void SetItemsToBuy(IReadOnlyList<ItemRowData> items, bool showWeight, bool showPrice)
        {
            _itemsToBuyTradeContainer.SetItems(items, showWeight, showPrice);
        }

        public void SetItemsToSell(IReadOnlyList<ItemRowData> items, bool showWeight, bool showPrice)
        {
            _itemsToSellTradeContainer.SetItems(items, showWeight, showPrice);
        }

        public void SetPlayerMoney(int value)
        {
            _playerTradeContainer.SetAdditionalHeaderText(value.ToString(), value);
        }

        public void SetTradeWeightInfo(string text, bool warning, params object[] args)
        {
            SetAdditionalHeaderState(true);
            SetAdditionalHeaderText(text, args);
            SetAdditionalHeaderHighlight(warning);
        }

        public void SetNpcMoney(int value)
        {
            _npcTradeContainer.SetAdditionalHeaderText(value.ToString(), value);
        }

        public void BindTradeAction(Action action)
        {
            UnbindTradeAction();
            if (action == null)
                return;

            _tradeHandler = () => action.Invoke();
            _tradeButton.onClick.AddListener(_tradeHandler);
        }

        public void UnbindTradeAction()
        {
            if (_tradeHandler == null)
                return;

            _tradeButton.onClick.RemoveListener(_tradeHandler);
            _tradeHandler = null;
        }

        public void SetTradeInteractable(bool state)
        {
            _tradeButton.interactable = state;
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
                return;

            _hasPlayerItemsHash = false;
            _hasNpcItemsHash = false;
            _hasBuyItemsHash = false;
            _hasSellItemsHash = false;
            CacheViews();
            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
            _viewModel.Navigate += HandleNavigate;
            _viewModel.Submit += HandleSubmit;
            _viewModel.SubmitAll += HandleSubmitAll;
            _viewModel.NextArea += HandleNextArea;
            _viewModel.PrevArea += HandlePrevArea;

            BindTradeAction(_viewModel.ExecuteTrade);
            BindItemViews();
            SetActiveArea(TradeArea.Player);
        }

        private void UnsubscribeViewModel()
        {
            if (_viewModel == null)
                return;

            _stateSubscription?.Dispose();
            _stateSubscription = null;

            _viewModel.Navigate -= HandleNavigate;
            _viewModel.Submit -= HandleSubmit;
            _viewModel.SubmitAll -= HandleSubmitAll;
            _viewModel.NextArea -= HandleNextArea;
            _viewModel.PrevArea -= HandlePrevArea;

            UnbindTradeAction();
            UnbindItemViews();
        }

        private void ApplyState(TradeViewState state)
        {
            if (!_hasPlayerItemsHash || _lastPlayerItemsHash != state.PlayerItemsHash)
            {
                _lastPlayerItemsHash = state.PlayerItemsHash;
                _hasPlayerItemsHash = true;
                SetPlayerItems(state.PlayerItems, showWeight: false, showPrice: true);
            }
            if (!_hasNpcItemsHash || _lastNpcItemsHash != state.NpcItemsHash)
            {
                _lastNpcItemsHash = state.NpcItemsHash;
                _hasNpcItemsHash = true;
                SetNpcItems(state.NpcItems, showWeight: false, showPrice: true);
            }
            if (!_hasBuyItemsHash || _lastBuyItemsHash != state.BuyItemsHash)
            {
                _lastBuyItemsHash = state.BuyItemsHash;
                _hasBuyItemsHash = true;
                SetItemsToBuy(state.BuyItems, showWeight: false, showPrice: true);
            }
            if (!_hasSellItemsHash || _lastSellItemsHash != state.SellItemsHash)
            {
                _lastSellItemsHash = state.SellItemsHash;
                _hasSellItemsHash = true;
                SetItemsToSell(state.SellItems, showWeight: false, showPrice: true);
            }

            SetPlayerMoney(state.PlayerMoney);
            SetNpcMoney(state.NpcMoney);

            _itemsToBuyTradeContainer.SetAdditionalHeaderText(state.BuyTotal.ToString(), state.BuyTotal);
            _itemsToSellTradeContainer.SetAdditionalHeaderText(state.SellTotal.ToString(), state.SellTotal);

            SetTradeInteractable(state.PlayerEnoughFunds);
            _itemsToBuyTradeContainer.SetAdditionalHeaderHighlight(!state.PlayerEnoughFunds);
            _itemsToSellTradeContainer.SetAdditionalHeaderHighlight(!state.NpcEnoughFunds);

            string weightFallback = state.MaxWeight > 0f
                ? $"{state.ProjectedWeight:0.##} / {state.MaxWeight:0.##}"
                : $"{state.ProjectedWeight:0.##}";
            SetTradeWeightInfo(weightFallback, state.WeightWarning, state.ProjectedWeight, state.MaxWeight);

            if (!string.IsNullOrWhiteSpace(state.NpcName))
                _npcTradeContainer.SetMainHeaderText(state.NpcName, state.NpcName);

            GetActiveView()?.EnsureSelection();
        }

        private void CacheViews()
        {
            _playerItemsView = _playerTradeContainer.ItemsView;
            _npcItemsView = _npcTradeContainer.ItemsView;
            _buyItemsView = _itemsToBuyTradeContainer.ItemsView;
            _sellItemsView = _itemsToSellTradeContainer.ItemsView;
        }

        private void BindItemViews()
        {
            BindItemsView(_playerItemsView, TradeArea.Player, HandlePlayerActivated);
            BindItemsView(_npcItemsView, TradeArea.Npc, HandleNpcActivated);
            BindItemsView(_buyItemsView, TradeArea.Buy, HandleBuyActivated);
            BindItemsView(_sellItemsView, TradeArea.Sell, HandleSellActivated);
        }

        private void BindItemsView(ItemsView view, TradeArea area, Action<ItemRowData, bool> activateAction)
        {
            if (view == null)
                return;

            Action<ItemRowData> selectHandler = _ => SetActiveArea(area);
            Action<ItemRowData, bool> activateHandler = (row, addAll) => activateAction.Invoke(row, addAll);

            switch (area)
            {
                case TradeArea.Player:
                    _playerSelectHandler = selectHandler;
                    _playerActivateHandler = activateHandler;
                    break;
                case TradeArea.Npc:
                    _npcSelectHandler = selectHandler;
                    _npcActivateHandler = activateHandler;
                    break;
                case TradeArea.Buy:
                    _buySelectHandler = selectHandler;
                    _buyActivateHandler = activateHandler;
                    break;
                case TradeArea.Sell:
                    _sellSelectHandler = selectHandler;
                    _sellActivateHandler = activateHandler;
                    break;
            }

            view.ItemSelected += selectHandler;
            view.ItemActivated += activateHandler;
            view.BindAction(() =>
            {
                SetActiveArea(area);
                view.TryActivateSelected(addAll: false);
            });
        }

        private void UnbindItemViews()
        {
            UnbindItemsView(_playerItemsView, _playerSelectHandler, _playerActivateHandler);
            UnbindItemsView(_npcItemsView, _npcSelectHandler, _npcActivateHandler);
            UnbindItemsView(_buyItemsView, _buySelectHandler, _buyActivateHandler);
            UnbindItemsView(_sellItemsView, _sellSelectHandler, _sellActivateHandler);
        }

        private void UnbindItemsView(ItemsView view, Action<ItemRowData> selectHandler, Action<ItemRowData, bool> activateHandler)
        {
            if (view == null)
                return;

            if (selectHandler != null)
                view.ItemSelected -= selectHandler;
            if (activateHandler != null)
                view.ItemActivated -= activateHandler;

            view.UnbindAction();
        }

        private void SetActiveArea(TradeArea area)
        {
            ItemsView previous = GetActiveView();
            _activeArea = area;
            ItemsView current = GetActiveView();
            if (previous != null && previous != current)
                previous.ClearSelection();
            current?.EnsureSelection();
        }

        private ItemsView GetActiveView()
        {
            return _activeArea switch
            {
                TradeArea.Player => _playerItemsView,
                TradeArea.Npc => _npcItemsView,
                TradeArea.Buy => _buyItemsView,
                TradeArea.Sell => _sellItemsView,
                _ => null
            };
        }

        private void HandleNavigate(Vector2 value)
        {
            GetActiveView()?.HandleNavigate(value);
        }

        private void HandleSubmit()
        {
            GetActiveView()?.TryActivateSelected(addAll: false);
        }

        private void HandleSubmitAll()
        {
            GetActiveView()?.TryActivateSelected(addAll: true);
        }

        private void HandleNextArea()
        {
            SetActiveArea(_activeArea switch
            {
                TradeArea.Player => TradeArea.Npc,
                TradeArea.Npc => TradeArea.Buy,
                TradeArea.Buy => TradeArea.Sell,
                _ => TradeArea.Player
            });
        }

        private void HandlePrevArea()
        {
            SetActiveArea(_activeArea switch
            {
                TradeArea.Player => TradeArea.Sell,
                TradeArea.Npc => TradeArea.Player,
                TradeArea.Buy => TradeArea.Npc,
                _ => TradeArea.Buy
            });
        }

        private void HandlePlayerActivated(ItemRowData row, bool addAll)
        {
            if (string.IsNullOrWhiteSpace(row.ItemId))
                return;

            _viewModel.MoveToSell(row.ItemId, addAll);
        }

        private void HandleNpcActivated(ItemRowData row, bool addAll)
        {
            if (string.IsNullOrWhiteSpace(row.ItemId))
                return;

            _viewModel.MoveToBuy(row.ItemId, addAll);
        }

        private void HandleBuyActivated(ItemRowData row, bool addAll)
        {
            if (string.IsNullOrWhiteSpace(row.ItemId))
                return;

            _viewModel.ReturnFromBuy(row.ItemId, addAll);
        }

        private void HandleSellActivated(ItemRowData row, bool addAll)
        {
            if (string.IsNullOrWhiteSpace(row.ItemId))
                return;

            _viewModel.ReturnFromSell(row.ItemId, addAll);
        }

        private void BindLocalization()
        {
            _tradeButtonHandle?.Dispose();
            _tradeButtonHandle = LocalizationHelper.BindText(_tradeButtonText, _tradeButtonLocalizedString, $"{name}.TradeButton");
        }
    }
}

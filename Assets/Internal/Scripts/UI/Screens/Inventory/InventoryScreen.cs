using System;
using System.Collections.Generic;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screen.ViewModel;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Inventory
{
    public class InventoryScreen : PopupScreen
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _actionButtonText;
        [SerializeField] private TextMeshProUGUI _moneyHeaderText;
        [Header("Buttons")]
        [SerializeField] private Button _actionButton;
        [Header("Content")]
        [SerializeField] private ItemsView _itemsView;
        [Header("LocalizedStrings")]
        [SerializeField] private LocalizedString _actionButtonLocalizedString;
        [SerializeField] private LocalizedString _moneyHeaderLocalizedString;

        public ItemsView ItemsView => _itemsView;

        private InventoryScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private int _lastItemsHash;
        private bool _hasItemsHash;
        private UnityAction _actionHandler;
        private LocalizationHelper.LocalizedTextHandle _actionHandle;
        private LocalizationHelper.LocalizedTextHandle _moneyHandle;

        protected override void OnEnable()
        {
            base.OnEnable();
            BindLocalization();
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            UnsubscribeViewModel();
            base.OnDisable();
            _actionHandle?.Dispose();
            _actionHandle = null;
            _moneyHandle?.Dispose();
            _moneyHandle = null;
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as InventoryScreenViewModel;
            SubscribeViewModel();
        }

        public void SetMoney(int value)
        {
            string fallback = value.ToString();
            if (_moneyHandle != null)
                _moneyHandle.SetArguments(fallback, value);
            else
                _moneyHeaderText.text = fallback;
        }

        public void SetWeight(float current, float max)
        {
            string fallback = max > 0f
                ? $"{current:0.##} / {max:0.##}"
                : $"{current:0.##}";
            SetAdditionalHeaderText(fallback, current, max);
        }

        public void SetItems(IReadOnlyList<ItemRowData> items, bool showWeight, bool showPrice)
        {
            _itemsView.SetItems(items, showWeight, showPrice);
        }

        public void BindAction(Action action)
        {
            UnbindAction();
            if (action == null)
                return;

            _actionHandler = () => action.Invoke();
            _actionButton.onClick.AddListener(_actionHandler);
        }

        public void UnbindAction()
        {
            if (_actionHandler == null)
                return;

            _actionButton.onClick.RemoveListener(_actionHandler);
            _actionHandler = null;
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
                return;

            _hasItemsHash = false;
            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
            _viewModel.Navigate += HandleNavigate;
            _viewModel.Submit += HandleSubmit;
            _viewModel.SubmitAll += HandleSubmitAll;
            _viewModel.Action += HandleAction;

            BindAction(DropSelectedItem);
            _itemsView.BindAction(DropSelectedItem);
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
            _viewModel.Action -= HandleAction;

            UnbindAction();
            _itemsView.UnbindAction();
        }

        private void ApplyState(InventoryViewState state)
        {
            SetMoney(state.Money);
            if (!_hasItemsHash || _lastItemsHash != state.ItemsHash)
            {
                _lastItemsHash = state.ItemsHash;
                _hasItemsHash = true;
                SetItems(state.Items, showWeight: true, showPrice: true);
            }
            SetWeight(state.CurrentWeight, state.MaxWeight);
            _itemsView.EnsureSelection();
        }

        private void HandleNavigate(Vector2 value)
        {
            _itemsView.HandleNavigate(value);
        }

        private void HandleSubmit()
        {
            _itemsView.TryActivateSelected(addAll: false);
        }

        private void HandleSubmitAll()
        {
            _itemsView.TryActivateSelected(addAll: true);
        }

        private void HandleAction()
        {
            DropSelectedItem();
        }

        private void DropSelectedItem()
        {
            _itemsView.EnsureSelection();
            if (!_itemsView.TryGetSelected(out ItemRowData item))
                return;

            if (string.IsNullOrWhiteSpace(item.ItemId))
                return;

            _viewModel?.DropItem(item.ItemId, 1);
        }

        private void BindLocalization()
        {
            _actionHandle?.Dispose();
            _moneyHandle?.Dispose();
            _actionHandle = LocalizationHelper.BindText(_actionButtonText, _actionButtonLocalizedString, $"{name}.ActionButton");
            _moneyHandle = LocalizationHelper.BindText(_moneyHeaderText, _moneyHeaderLocalizedString, $"{name}.MoneyFormat");
        }
    }
}

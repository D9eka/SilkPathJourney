using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.Shared;
using Internal.Scripts.UI.Theme;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Workshop
{
    public class WorkshopScreen : PopupScreen
    {
        [Header("Indicators")]
        [SerializeField] private ResourceIndicator _moneyIndicator;
        [SerializeField] private ResourceIndicator _weightIndicator;
        [SerializeField] private ResourceIndicator _speedIndicator;

        [Header("Main Cart")]
        [SerializeField] private TextMeshProUGUI _mainCartHeaderText;
        [SerializeField] private LocalizedString _mainCartHeaderLocalized;
        [SerializeField] private MainCartCardView _mainCartCard;

        [Header("Extra Carts")]
        [SerializeField] private TextMeshProUGUI _extraCartsHeaderText;
        [SerializeField] private LocalizedString _extraCartsHeaderLocalized;
        [SerializeField] private Transform _extraCartsContent;
        [SerializeField] private ExtraCartCardView _extraCartPrefab;
        [SerializeField] private TextMeshProUGUI _slotsText;

        [Header("Upgrades")]
        [SerializeField] private Transform _upgradesContent;
        [SerializeField] private ServiceCardView _upgradeCardPrefab;

        [Header("Quest")]
        [SerializeField] private QuestCardView _questCardView;

        private WorkshopScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private IDisposable _questSlotSubscription;
        private LocalizationService.LocalizedTextHandle _mainCartHeaderHandle;
        private LocalizationService.LocalizedTextHandle _extraCartsHeaderHandle;
        private readonly List<ExtraCartCardView> _spawnedExtraCarts = new();
        private readonly List<ServiceCardView> _spawnedUpgrades = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            BindSectionHeaders();
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            UnsubscribeViewModel();
            _mainCartHeaderHandle?.Dispose();
            _mainCartHeaderHandle = null;
            _extraCartsHeaderHandle?.Dispose();
            _extraCartsHeaderHandle = null;
            base.OnDisable();
        }

        protected override void OnLocalizationReady()
        {
            base.OnLocalizationReady();
            BindSectionHeaders();
        }

        private void BindSectionHeaders()
        {
            _mainCartHeaderHandle?.Dispose();
            if (_mainCartHeaderText != null && _mainCartHeaderLocalized != null && Localization != null)
                _mainCartHeaderHandle = Localization.BindText(_mainCartHeaderText, _mainCartHeaderLocalized, "Workshop.MainCartHeader");

            _extraCartsHeaderHandle?.Dispose();
            if (_extraCartsHeaderText != null && _extraCartsHeaderLocalized != null && Localization != null)
                _extraCartsHeaderHandle = Localization.BindText(_extraCartsHeaderText, _extraCartsHeaderLocalized, "Workshop.ExtraCartsHeader");

        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as WorkshopScreenViewModel;
            SetupIcons();
            SubscribeViewModel();
        }

        private void SetupIcons()
        {
            var icons = _viewModel?.ResourceIcons;
            if (icons == null) return;
            _moneyIndicator?.SetIcon(icons.Get(ResourceType.Money)?.Icon);
            _weightIndicator?.SetIcon(icons.Get(ResourceType.Weight)?.Icon);
            _speedIndicator?.SetIcon(icons.Get(ResourceType.Speed)?.Icon);
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
                return;

            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
            _questSlotSubscription = _viewModel.QuestSlot.Subscribe(state =>
            {
                if (state.HasValue)
                    _questCardView.Initialize(state.Value.Description, () => _viewModel.OnQuestSlotTalk());
                else
                    _questCardView.Hide();
            });
        }

        private void UnsubscribeViewModel()
        {
            _stateSubscription?.Dispose();
            _stateSubscription = null;
            _questSlotSubscription?.Dispose();
            _questSlotSubscription = null;
        }

        private void ApplyState(WorkshopViewState state)
        {
            if (state == null) return;

            _moneyIndicator?.SetValue(state.PlayerMoney);
            _weightIndicator?.SetValue(state.WeightFormatted);
            _speedIndicator?.SetValue(state.SpeedFormatted);

            if (_slotsText != null)
                _slotsText.text = state.SlotsFormatted;

            ApplyMainCart(state.MainCart);
            RebuildExtraCarts(state.AvailableExtraCarts);
            RebuildUpgrades(state.Upgrades);

        }

        private void ApplyMainCart(MainCartViewData data)
        {
            if (_mainCartCard == null || _viewModel == null) return;

            string upgradeLabel = _viewModel.ResolveUpgradeLabel(data.UpgradeCost);
            string maxLabel = _viewModel.ResolveMaxUpgradeLabel();

            _mainCartCard.Initialize(data, upgradeLabel, maxLabel, HandleUpgradeMainCart);
        }

        private void RebuildExtraCarts(IReadOnlyList<ExtraCartViewData> extraCarts)
        {
            foreach (var item in _spawnedExtraCarts)
                Destroy(item.gameObject);
            _spawnedExtraCarts.Clear();

            if (extraCarts == null || _extraCartPrefab == null || _extraCartsContent == null)
                return;

            foreach (var data in extraCarts)
            {
                var instance = Instantiate(_extraCartPrefab, _extraCartsContent);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);

                string buyLabel = _viewModel.ResolveBuyLabel(data.BuyPrice);
                string sellLabel = _viewModel.ResolveSellLabel(data.SellPrice);

                instance.Initialize(data, buyLabel, sellLabel, HandleBuyExtraCart, HandleSellExtraCart);
                _spawnedExtraCarts.Add(instance);
            }
        }

        private void HandleUpgradeMainCart()
        {
            _viewModel?.UpgradeMainCart();
        }

        private void HandleBuyExtraCart(ExtraCartType type)
        {
            _viewModel?.BuyExtraCart(type);
        }

        private void HandleSellExtraCart(ExtraCartType type)
        {
            _viewModel?.SellExtraCart(type);
        }

        private void RebuildUpgrades(IReadOnlyList<UpgradeViewData> upgrades)
        {
            foreach (var item in _spawnedUpgrades)
                Destroy(item.gameObject);
            _spawnedUpgrades.Clear();

            if (upgrades == null || _upgradeCardPrefab == null || _upgradesContent == null)
                return;

            foreach (var data in upgrades)
            {
                var instance = Instantiate(_upgradeCardPrefab, _upgradesContent);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);

                var type = data.Type;
                instance.Initialize(data.Title, data.Description, data.ButtonText,
                    data.CanBuy && !data.AlreadyOwned, () => HandleBuyUpgrade(type));
                _spawnedUpgrades.Add(instance);
            }
        }

        private void HandleBuyUpgrade(CaravanUpgradeType type)
        {
            _viewModel?.BuyUpgrade(type);
        }
    }
}

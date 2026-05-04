using System;
using System.Collections.Generic;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Caravan
{
    public class CaravanScreen : PopupScreen
    {
        [Header("Header Indicators")]
        [SerializeField] private ResourceIndicator _moneyIndicator;
        [SerializeField] private ResourceIndicator _weightIndicator;

        [Header("Stats Bar")]
        [SerializeField] private ResourceIndicator _moraleIndicator;
        [SerializeField] private ResourceIndicator _dangerIndicator;
        [SerializeField] private ResourceIndicator _foodIndicator;
        [SerializeField] private ResourceIndicator _speedIndicator;
        [SerializeField] private ResourceIndicator _companionPayIndicator;
        [SerializeField] private ResourceIndicator _healthIndicator;

        [Header("Companions")]
        [SerializeField] private CompanionCardView _companionPrefab;
        [SerializeField] private RectTransform _companionContent;
        [SerializeField] private TextMeshProUGUI _companionsHeaderText;
        [SerializeField] private LocalizedString _companionsHeaderLocalized;
        [SerializeField] private GameObject _companionEmptyPlaceholder;
        [SerializeField] private TextMeshProUGUI _companionEmptyText;
        [SerializeField] private LocalizedString _companionEmptyLocalizedString;

        [Header("Animal")]
        [SerializeField] private AnimalInfoView _animalInfoView;

        [Header("Carts")]
        [SerializeField] private CartCardView _cartPrefab;
        [SerializeField] private RectTransform _cartContent;
        [SerializeField] private TextMeshProUGUI _cartsHeaderText;
        [SerializeField] private LocalizedString _cartsHeaderLocalized;

        private CaravanScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private LocalizationService.LocalizedTextHandle _emptyStateHandle;
        private LocalizationService.LocalizedTextHandle _companionsHeaderHandle;
        private LocalizationService.LocalizedTextHandle _cartsHeaderHandle;
        private readonly List<CompanionCardView> _spawnedCompanions = new();
        private readonly List<CartCardView> _spawnedCarts = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            BindEmptyStateLocalization();
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            UnsubscribeViewModel();
            _emptyStateHandle?.Dispose();
            _emptyStateHandle = null;
            _companionsHeaderHandle?.Dispose();
            _cartsHeaderHandle?.Dispose();
            base.OnDisable();
        }

        protected override void OnLocalizationReady()
        {
            base.OnLocalizationReady();
            BindEmptyStateLocalization();
            BindSectionHeaders();
        }

        private void BindEmptyStateLocalization()
        {
            _emptyStateHandle?.Dispose();
            if (_companionEmptyText != null && _companionEmptyLocalizedString != null && Localization != null)
                _emptyStateHandle = Localization.BindText(_companionEmptyText, _companionEmptyLocalizedString, $"{name}.EmptyState");
        }

        private void BindSectionHeaders()
        {
            _companionsHeaderHandle?.Dispose();
            if (_companionsHeaderText != null && _companionsHeaderLocalized != null && Localization != null)
                _companionsHeaderHandle = Localization.BindText(_companionsHeaderText, _companionsHeaderLocalized, "Caravan.CompanionsHeader");

            _cartsHeaderHandle?.Dispose();
            if (_cartsHeaderText != null && _cartsHeaderLocalized != null && Localization != null)
                _cartsHeaderHandle = Localization.BindText(_cartsHeaderText, _cartsHeaderLocalized, "Caravan.CartsHeader");
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as CaravanScreenViewModel;
            SetupIcons();
            SubscribeViewModel();
        }

        private void SetupIcons()
        {
            var icons = _viewModel?.ResourceIcons;
            if (icons == null) return;
            _moneyIndicator?.SetIcon(icons.Get(ResourceType.Money)?.Icon);
            _weightIndicator?.SetIcon(icons.Get(ResourceType.Weight)?.Icon);
            _moraleIndicator?.SetIcon(icons.Get(ResourceType.Morale)?.Icon);
            _dangerIndicator?.SetIcon(icons.Get(ResourceType.Danger)?.Icon);
            _foodIndicator?.SetIcon(icons.Get(ResourceType.Food)?.Icon);
            _speedIndicator?.SetIcon(icons.Get(ResourceType.Speed)?.Icon);
            _companionPayIndicator?.SetIcon(icons.Get(ResourceType.CompanionPay)?.Icon);
            _healthIndicator?.SetIcon(icons.Get(ResourceType.Health)?.Icon);
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
                return;

            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
        }

        private void UnsubscribeViewModel()
        {
            if (_viewModel == null)
                return;

            _stateSubscription?.Dispose();
            _stateSubscription = null;
        }

        private void ApplyState(CaravanViewState state)
        {
            if (state == null) return;

            _moneyIndicator?.SetValue(state.PlayerMoney);
            _weightIndicator?.SetValue(state.WeightFormatted);

            _moraleIndicator?.SetValue(state.MoraleFormatted);
            _dangerIndicator?.SetValue(state.DangerFormatted);
            _foodIndicator?.SetValue(state.FoodFormatted);
            _speedIndicator?.SetValue(state.SpeedFormatted);
            _companionPayIndicator?.SetValue(state.CompanionPayFormatted);
            _healthIndicator?.SetValue(state.HealthFormatted);

            RebuildCompanions(state.Companions);
            RebuildCarts(state.Carts);

            if (_animalInfoView != null)
                _animalInfoView.Initialize(state.Animal, HandleHealAnimal);
        }

        private void RebuildCompanions(IReadOnlyList<CompanionCardData> companions)
        {
            foreach (var item in _spawnedCompanions)
                Destroy(item.gameObject);
            _spawnedCompanions.Clear();

            int count = companions?.Count ?? 0;
            _companionEmptyPlaceholder?.SetActive(count == 0);

            if (companions == null || _companionPrefab == null || _companionContent == null)
                return;

            foreach (var data in companions)
            {
                var instance = Instantiate(_companionPrefab, _companionContent);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                instance.Initialize(data, HandleCompanionAction);
                _spawnedCompanions.Add(instance);
            }
        }

        private void RebuildCarts(IReadOnlyList<CartViewData> carts)
        {
            foreach (var item in _spawnedCarts)
                Destroy(item.gameObject);
            _spawnedCarts.Clear();

            if (carts == null || _cartPrefab == null || _cartContent == null)
                return;

            foreach (var data in carts)
            {
                var instance = Instantiate(_cartPrefab, _cartContent);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                instance.Initialize(data, HandleCartRepair, HandleCartDiscard);
                _spawnedCarts.Add(instance);
            }
        }

        private void HandleCompanionAction(int index, CompanionCardAction action)
        {
            if (action == CompanionCardAction.Fire)
                _viewModel?.FireCompanion(index);
        }

        private void HandleCartRepair(int index)
        {
            _viewModel?.RepairCart(index);
        }

        private void HandleCartDiscard(int index)
        {
            _viewModel?.DiscardCart(index);
        }

        private void HandleHealAnimal()
        {
            _viewModel?.HealAnimal();
        }
    }
}

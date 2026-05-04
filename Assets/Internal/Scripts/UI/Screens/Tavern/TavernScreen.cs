using System;
using System.Collections.Generic;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.Shared;
using Internal.Scripts.UI.Theme;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Tavern
{
    public class TavernScreen : PopupScreen
    {
        [Header("Money")]
        [SerializeField] private ResourceIndicator _moneyIndicator;

        [Header("Rumors")]
        [SerializeField] private RumorView _rumorView;

        [Header("Road Info")]
        [SerializeField] private TextMeshProUGUI _roadInfoHeaderText;
        [SerializeField] private LocalizedString _roadInfoHeaderLocalized;
        [SerializeField] private ServiceCardView _roadInfoPrefab;
        [SerializeField] private RectTransform _roadInfoContent;
        [SerializeField] private GameObject _roadInfoScrollView;
        [SerializeField] private GameObject _roadInfoEmptyPlaceholder;
        [SerializeField] private TextMeshProUGUI _roadInfoEmptyText;
        [SerializeField] private LocalizedString _roadInfoEmptyLocalized;

        [Header("Companions")]
        [SerializeField] private RectTransform _companionContent;
        [SerializeField] private CompanionHireCardView _companionPrefab;
        [SerializeField] private TextMeshProUGUI _slotsText;
        [SerializeField] private TextMeshProUGUI _companionsHeaderText;
        [SerializeField] private LocalizedString _companionsHeaderLocalized;

        [Header("Quest")]
        [SerializeField] private QuestCardView _questCardView;

        private TavernScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private IDisposable _questSlotSubscription;
        private LocalizationService.LocalizedTextHandle _roadInfoEmptyHandle;
        private readonly List<CompanionHireCardView> _spawnedCompanions = new();
        private readonly List<ServiceCardView> _spawnedTavernCards = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            BindRoadInfoEmptyLocalization();
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            UnsubscribeViewModel();
            _roadInfoEmptyHandle?.Dispose();
            _roadInfoEmptyHandle = null;
            base.OnDisable();
        }

        protected override void OnLocalizationReady()
        {
            base.OnLocalizationReady();
            BindRoadInfoEmptyLocalization();
        }

        private void BindRoadInfoEmptyLocalization()
        {
            _roadInfoEmptyHandle?.Dispose();
            if (_roadInfoEmptyText != null && _roadInfoEmptyLocalized != null && Localization != null)
                _roadInfoEmptyHandle = Localization.BindText(_roadInfoEmptyText, _roadInfoEmptyLocalized, $"{name}.RoadInfoEmpty");
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as TavernScreenViewModel;
            SetupIcons();
            ResolveStaticTexts();
            SubscribeViewModel();
        }

        private void ResolveStaticTexts()
        {
            if (_roadInfoHeaderText != null)
                _roadInfoHeaderText.text = _roadInfoHeaderLocalized != null
                    ? LocalizationService.ResolveString(_roadInfoHeaderLocalized, "UI.Tavern.RoadInfo.Header.Main", "Tavern.RoadInfoHeader")
                    : "UI.Tavern.RoadInfo.Header.Main";

            if (_companionsHeaderText != null)
                _companionsHeaderText.text = _companionsHeaderLocalized != null
                    ? LocalizationService.ResolveString(_companionsHeaderLocalized, "UI.Tavern.Companions.Header.Main", "Tavern.CompanionsHeader")
                    : "UI.Tavern.Companions.Header.Main";
        }

        private void SetupIcons()
        {
            var icons = _viewModel?.ResourceIcons;
            if (icons == null) return;
            _moneyIndicator?.SetIcon(icons.Get(ResourceType.Money)?.Icon);
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

        private void ApplyState(TavernViewState state)
        {
            if (state == null) return;

            _moneyIndicator?.SetValue(state.PlayerMoney);

            if (_slotsText != null)
                _slotsText.text = state.SlotsFormatted;

            if (_rumorView != null)
                _rumorView.Initialize(state.RumorsText);

            RebuildTavernCards(state.RoadInfos, state.PriceTips);
            RebuildCompanions(state.AvailableCompanions);
        }

        private void RebuildTavernCards(IReadOnlyList<RoadInfoEntry> roadInfos, IReadOnlyList<PriceTipEntry> priceTips)
        {
            foreach (var item in _spawnedTavernCards)
                Destroy(item.gameObject);
            _spawnedTavernCards.Clear();

            int roadCount = roadInfos?.Count ?? 0;
            int tipCount = priceTips?.Count ?? 0;
            bool hasAny = roadCount > 0 || tipCount > 0;

            _roadInfoScrollView?.SetActive(hasAny);
            _roadInfoEmptyPlaceholder?.SetActive(!hasAny);

            if (!hasAny || _roadInfoPrefab == null || _roadInfoContent == null)
                return;

            for (int i = 0; i < roadCount; i++)
            {
                var info = roadInfos[i];
                int idx = info.Index;
                SpawnTavernCard(info.RouteName, info.Description, info.ButtonText, info.CanBuy,
                    () => _viewModel?.BuyRoadInfo(idx));
            }

            for (int i = 0; i < tipCount; i++)
            {
                var tip = priceTips[i];
                int idx = tip.Index;
                SpawnTavernCard(tip.CityName, tip.Description, tip.ButtonText, tip.CanBuy,
                    () => _viewModel?.BuyPriceTipForCity(idx));
            }
        }

        private void SpawnTavernCard(string title, string description, string buttonText, bool canBuy, Action onBuy)
        {
            var instance = Instantiate(_roadInfoPrefab, _roadInfoContent);
            instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
            instance.Initialize(title, description, buttonText, canBuy, onBuy);
            _spawnedTavernCards.Add(instance);
        }

        private void RebuildCompanions(IReadOnlyList<CompanionHireData> companions)
        {
            foreach (var item in _spawnedCompanions)
                Destroy(item.gameObject);
            _spawnedCompanions.Clear();

            if (companions == null || _companionPrefab == null || _companionContent == null)
                return;

            foreach (var data in companions)
            {
                var instance = Instantiate(_companionPrefab, _companionContent);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                instance.Initialize(data, HandleHireClicked);
                _spawnedCompanions.Add(instance);
            }
        }

        private void HandleHireClicked(int index)
        {
            _viewModel?.HireCompanion(index);
        }
    }
}

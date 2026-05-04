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

namespace Internal.Scripts.UI.Screens.Barracks
{
    public class BarracksScreen : PopupScreen
    {
        [SerializeField] private ServiceCardView _itemPrefab;
        [SerializeField] private RectTransform _content;
        [SerializeField] private TextMeshProUGUI _additionalHeaderText;
        [SerializeField] private ResourceIndicator _moneyIndicator;
        [SerializeField] private ServiceCardView _eliteGuardCard;
        [SerializeField] private QuestCardView _questCard;

        [Header("Localization")]
        [SerializeField] private LocalizedString _additionalHeaderLocalizedString;

        private BarracksScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private IDisposable _questSlotSubscription;
        private LocalizationService.LocalizedTextHandle _additionalHeaderHandle;
        private readonly List<ServiceCardView> _spawnedItems = new();

        public QuestCardView QuestCard => _questCard;

        protected override void OnEnable()
        {
            base.OnEnable();
            BindStaticTexts();
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            _additionalHeaderHandle?.Dispose();
            _additionalHeaderHandle = null;
            UnsubscribeViewModel();
            base.OnDisable();
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as BarracksScreenViewModel;
            SetupIcons();
            BindStaticTexts();
            SubscribeViewModel();
        }

        private void SetupIcons()
        {
            var icons = _viewModel?.ResourceIcons;
            if (icons == null) return;
            _moneyIndicator?.SetIcon(icons.Get(ResourceType.Money)?.Icon);
        }

        private void BindStaticTexts()
        {
            if (Localization == null) return;
            _additionalHeaderHandle?.Dispose();
            _additionalHeaderHandle = Localization.BindText(_additionalHeaderText, _additionalHeaderLocalizedString, "Barracks.Header");
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
                return;

            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
            _questSlotSubscription = _viewModel.QuestSlot.Subscribe(state =>
            {
                if (state.HasValue)
                    _questCard?.Initialize(state.Value.Description, () => _viewModel.OnQuestSlotTalk());
                else
                    _questCard?.Hide();
            });
        }

        private void UnsubscribeViewModel()
        {
            _stateSubscription?.Dispose();
            _stateSubscription = null;
            _questSlotSubscription?.Dispose();
            _questSlotSubscription = null;
        }

        private void ApplyState(BarracksViewState state)
        {
            if (state == null) return;

            _moneyIndicator?.SetValue(state.PlayerMoney);
            RebuildItems(state.ScrollEntries);

            var eg = state.EliteGuard;
            _eliteGuardCard?.Initialize(eg.Name, eg.Description, eg.ButtonText,
                eg.CanAfford && !eg.IsOwned, () => _viewModel?.UpgradeEliteGuard());
        }

        private void RebuildItems(IReadOnlyList<BarracksEntry> entries)
        {
            foreach (var item in _spawnedItems)
                Destroy(item.gameObject);
            _spawnedItems.Clear();

            if (entries == null || _itemPrefab == null || _content == null)
                return;

            foreach (var entry in entries)
            {
                var instance = Instantiate(_itemPrefab, _content);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                var kind = entry.Kind;
                instance.Initialize(entry.Name, entry.Description, entry.ButtonText,
                    entry.CanAfford, () => _viewModel?.Action(kind));
                _spawnedItems.Add(instance);
            }
        }
    }
}

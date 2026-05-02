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

namespace Internal.Scripts.UI.Screens.Healer
{
    public class HealerScreen : PopupScreen
    {
        [SerializeField] private ServiceCardView _itemPrefab;
        [SerializeField] private RectTransform _content;
        [SerializeField] private TextMeshProUGUI _attentionText;
        [SerializeField] private TextMeshProUGUI _additionalHeaderText;
        [SerializeField] private ResourceIndicator _moneyIndicator;

        [Header("Localization")]
        [SerializeField] private LocalizedString _additionalHeaderLocalizedString;
        [SerializeField] private LocalizedString _attentionHealthyLocalizedString;
        [SerializeField] private LocalizedString _attentionInjuredLocalizedString;

        private HealerScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private LocalizationService.LocalizedTextHandle _additionalHeaderHandle;
        private readonly List<ServiceCardView> _spawnedItems = new();

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
            _viewModel = viewModel as HealerScreenViewModel;
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
            _additionalHeaderHandle = Localization.BindText(_additionalHeaderText, _additionalHeaderLocalizedString, "Healer.Header");
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
                return;

            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
        }

        private void UnsubscribeViewModel()
        {
            if (_viewModel == null) return;

            _stateSubscription?.Dispose();
            _stateSubscription = null;
        }

        private void ApplyState(HealerViewState state)
        {
            if (state == null) return;

            _moneyIndicator?.SetValue(state.PlayerMoney);

            var localized = state.AnyInjured ? _attentionInjuredLocalizedString : _attentionHealthyLocalizedString;
            _attentionText.text = LocalizationService.ResolveString(localized, state.AnyInjured ? "Companions need healing" : "All companions are healthy", "Healer.Attention");

            RebuildItems(state.Entries);
        }

        private void RebuildItems(IReadOnlyList<HealerEntry> entries)
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
                int index = entry.CompanionIndex;
                instance.Initialize(entry.Name, entry.StatusText, entry.ButtonText,
                    entry.IsInjured && entry.CanAfford, () => HandleHealClicked(index));
                _spawnedItems.Add(instance);
            }
        }

        private void HandleHealClicked(int companionIndex)
        {
            _viewModel?.Heal(companionIndex);
        }
    }
}

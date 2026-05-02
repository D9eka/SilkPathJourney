using System;
using System.Collections.Generic;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Journal
{
    public class JournalScreen : PopupScreen
    {
        [SerializeField] private JournalDayItemView _itemPrefab;
        [SerializeField] private RectTransform _content;
        [SerializeField] private GameObject _emptyStatePlaceholder;
        [SerializeField] private TextMeshProUGUI _emptyStateText;

        [Header("Localization")]
        [SerializeField] private LocalizedString _emptyStateLocalizedString;

        private JournalScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private LocalizationService.LocalizedTextHandle _emptyStateHandle;
        private readonly List<JournalDayItemView> _spawnedItems = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            BindEmptyStateLocalization();
            SubscribeViewModel();
        }

        protected override void OnLocalizationReady()
        {
            base.OnLocalizationReady();
            BindEmptyStateLocalization();
        }

        private void BindEmptyStateLocalization()
        {
            _emptyStateHandle?.Dispose();
            if (_emptyStateText != null && _emptyStateLocalizedString != null && Localization != null)
                _emptyStateHandle = Localization.BindText(_emptyStateText, _emptyStateLocalizedString, "Journal.Empty");
        }

        protected override void OnDisable()
        {
            _emptyStateHandle?.Dispose();
            _emptyStateHandle = null;
            _stateSubscription?.Dispose();
            _stateSubscription = null;
            base.OnDisable();
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as JournalScreenViewModel;
            SubscribeViewModel();
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
                return;

            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
        }

        private void ApplyState(JournalViewState state)
        {
            if (state == null) return;

            bool isEmpty = state.Days == null || state.Days.Count == 0;
            _emptyStatePlaceholder.SetActive(isEmpty);

            RebuildItems(state.Days);
        }

        private void RebuildItems(IReadOnlyList<JournalDayEntry> days)
        {
            foreach (var item in _spawnedItems)
                Destroy(item.gameObject);
            _spawnedItems.Clear();

            if (days == null || _itemPrefab == null || _content == null)
                return;

            foreach (var day in days)
            {
                var instance = Instantiate(_itemPrefab, _content);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                instance.Bind(_viewModel.IconCatalog);
                instance.Initialize(day);
                _spawnedItems.Add(instance);
            }
        }
    }
}

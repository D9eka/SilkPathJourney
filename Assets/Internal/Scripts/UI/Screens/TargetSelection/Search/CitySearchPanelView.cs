using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Theme;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.TargetSelection.Search
{
    public class CitySearchPanelView : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private TextMeshProUGUI _inputHeader;
        [SerializeField] private TMP_InputField _searchInput;
        [SerializeField] private Button _clearButton;
        [SerializeField] private GameObject _clearButtonObject;
        [SerializeField] private TextMeshProUGUI _hintText;
        [SerializeField] private TextMeshProUGUI _placeholderText;

        [Header("Filters")]
        [SerializeField] private BuildingFilterButton _questFilterButton;
        [SerializeField] private BuildingFilterButton[] _filterButtons;

        [Header("Results")]
        [SerializeField] private GameObject _scrollViewRoot;
        [SerializeField] private TextMeshProUGUI _resultsHeader;
        [SerializeField] private Transform _scrollContent;
        [SerializeField] private CityRowView _cityRowPrefab;

        [Header("Empty State")]
        [SerializeField] private GameObject _emptyStateSection;
        [SerializeField] private TextMeshProUGUI _emptyText;

        [Header("Localization")]
        [SerializeField] private LocalizedString _inputHeaderLocalized;
        [SerializeField] private LocalizedString _placeholderLocalized;
        [SerializeField] private LocalizedString _hintLocalized;
        [SerializeField] private LocalizedString _resultsHeaderLocalized;
        [SerializeField] private LocalizedString _emptyTextLocalized;

        private CitySearchPanelViewModel _viewModel;
        private LocalizationService _localization;
        private IDisposable _stateSubscription;
        private IDisposable _visibilitySubscription;
        private readonly List<CityRowView> _activeRows = new();
        private ComponentPool<CityRowView> _rowPool;
        private bool _filtersBound;

        private LocalizationService.LocalizedTextHandle _inputHeaderHandle;
        private LocalizationService.LocalizedTextHandle _placeholderHandle;
        private LocalizationService.LocalizedTextHandle _hintHandle;
        private LocalizationService.LocalizedTextHandle _emptyHandle;
        private LocalizationService.LocalizedTextHandle _resultsHeaderHandle;
        private int _resultsCount;

        public void SetLocalization(LocalizationService localization)
        {
            _localization = localization;
            BindStaticLocalization();
            foreach (var row in _activeRows)
                row?.SetLocalization(localization);
        }

        public void Bind(CitySearchPanelViewModel viewModel)
        {
            Unbind();
            _viewModel = viewModel;
            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
            _visibilitySubscription = _viewModel.IsVisible.Subscribe(OnVisibilityChanged);
            EnsureRowPool();
            WireButtons();
            BindFilterButtons();
            BindStaticLocalization();
            ResetSearchInputState();
        }

        public void Unbind()
        {
            UnwireButtons();
            UnbindFilterButtons();
            _stateSubscription?.Dispose();
            _stateSubscription = null;
            _visibilitySubscription?.Dispose();
            _visibilitySubscription = null;
            _viewModel = null;
        }

        private void OnDestroy()
        {
            Unbind();
            ReleaseRows();
            DisposeStaticHandles();
        }

        private void EnsureRowPool()
        {
            if (_rowPool == null && _cityRowPrefab != null && _scrollContent != null)
                _rowPool = new ComponentPool<CityRowView>(_cityRowPrefab, _scrollContent);
        }

        private void ResetSearchInputState()
        {
            if (_searchInput != null)
                _searchInput.text = string.Empty;
            if (_clearButtonObject != null)
                _clearButtonObject.SetActive(false);
        }

        private void OnVisibilityChanged(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void BindStaticLocalization()
        {
            if (_localization == null) return;

            DisposeStaticHandles();

            if (_inputHeader != null && _inputHeaderLocalized != null)
                _inputHeaderHandle = _localization.BindText(
                    _inputHeader, _inputHeaderLocalized, "CitySearch.Header.Input");

            TextMeshProUGUI placeholder = _placeholderText != null
                ? _placeholderText
                : (_searchInput != null ? _searchInput.placeholder as TextMeshProUGUI : null);
            if (placeholder != null && _placeholderLocalized != null)
                _placeholderHandle = _localization.BindText(
                    placeholder, _placeholderLocalized, "CitySearch.Input.Placeholder");

            if (_hintText != null && _hintLocalized != null)
                _hintHandle = _localization.BindText(
                    _hintText, _hintLocalized, "CitySearch.Hint");

            if (_emptyText != null && _emptyTextLocalized != null)
                _emptyHandle = _localization.BindText(
                    _emptyText, _emptyTextLocalized, "CitySearch.Empty");

            BindResultsHeader();
        }

        private void BindResultsHeader()
        {
            _resultsHeaderHandle?.Dispose();
            if (_resultsHeader == null || _resultsHeaderLocalized == null || _localization == null)
                return;
            _resultsHeaderHandle = _localization.BindText(
                _resultsHeader, _resultsHeaderLocalized,
                "CitySearch.Header.Results", fallback: null, postProcess: null, _resultsCount);
        }

        private void DisposeStaticHandles()
        {
            _inputHeaderHandle?.Dispose();
            _inputHeaderHandle = null;
            _placeholderHandle?.Dispose();
            _placeholderHandle = null;
            _hintHandle?.Dispose();
            _hintHandle = null;
            _emptyHandle?.Dispose();
            _emptyHandle = null;
            _resultsHeaderHandle?.Dispose();
            _resultsHeaderHandle = null;
        }

        private void WireButtons()
        {
            if (_searchInput != null)
            {
                _searchInput.onValueChanged.AddListener(OnSearchChanged);
                _searchInput.onSelect.AddListener(OnSearchFocused);
                _searchInput.onDeselect.AddListener(OnSearchUnfocused);
            }

            if (_clearButton != null)
                _clearButton.onClick.AddListener(OnClearClicked);
        }

        private void UnwireButtons()
        {
            if (_searchInput != null)
            {
                _searchInput.onValueChanged.RemoveListener(OnSearchChanged);
                _searchInput.onSelect.RemoveListener(OnSearchFocused);
                _searchInput.onDeselect.RemoveListener(OnSearchUnfocused);
            }

            if (_clearButton != null)
                _clearButton.onClick.RemoveListener(OnClearClicked);
        }

        private void BindFilterButtons()
        {
            if (_filtersBound || _viewModel == null)
                return;

            var catalog = _viewModel.BuildingFilterCatalog;
            var tooltip = _viewModel.TooltipService;

            if (_questFilterButton != null)
            {
                string questTooltip = LocalizationService.Resolve(
                    "UI", "UI.CitySearch.Filter.Quest", "Quests");
                _questFilterButton.InitializeWithTooltip(catalog, tooltip, questTooltip,
                    () => _viewModel?.ToggleQuestFilter());
            }

            if (_filterButtons != null)
            {
                foreach (BuildingFilterButton btn in _filterButtons)
                {
                    if (btn == null)
                        continue;

                    BuildingId id = btn.Building;
                    btn.Initialize(catalog, tooltip, () => _viewModel?.ToggleFilter(id));
                }
            }

            _filtersBound = true;
        }

        private void UnbindFilterButtons()
        {
            if (!_filtersBound)
                return;

            _questFilterButton?.Unbind();

            if (_filterButtons != null)
            {
                foreach (BuildingFilterButton btn in _filterButtons)
                    btn?.Unbind();
            }

            _filtersBound = false;
        }

        private void ApplyState(CitySearchViewState state)
        {
            if (state == null) return;

            bool hasSearchActive = state.ShowResults || state.ShowEmpty;

            if (_scrollViewRoot != null)
                _scrollViewRoot.SetActive(hasSearchActive);

            if (_emptyStateSection != null)
                _emptyStateSection.SetActive(state.ShowEmpty);

            if (state.ShowResults)
            {
                if (_resultsCount != state.ResultsCount)
                {
                    _resultsCount = state.ResultsCount;
                    BindResultsHeader();
                }
                RebuildRows(state.Results);
            }
            else
            {
                ReleaseRows();
            }

            UpdateFilterHighlights(state);
        }

        private void RebuildRows(IReadOnlyList<CityRowData> results)
        {
            ReleaseRows();

            if (results == null || _rowPool == null)
                return;

            foreach (var data in results)
            {
                CityRowView row = _rowPool.Get();
                row.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                row.SetTooltipService(_viewModel?.TooltipService);
                row.SetLocalization(_localization);
                string cityId = data.CityId;
                row.Initialize(data, () => _viewModel?.NotifyCityClickedById(cityId));
                _activeRows.Add(row);
            }
        }

        private void ReleaseRows()
        {
            if (_rowPool == null)
            {
                _activeRows.Clear();
                return;
            }

            foreach (var row in _activeRows)
                _rowPool.Release(row);
            _activeRows.Clear();
        }

        private void UpdateFilterHighlights(CitySearchViewState state)
        {
            _questFilterButton?.SetActive(state.QuestFilterActive);

            if (_filterButtons == null)
                return;

            var active = state.ActiveFilters;
            foreach (BuildingFilterButton btn in _filterButtons)
            {
                if (btn == null)
                    continue;
                btn.SetActive(active != null && active.Contains(btn.Building));
            }
        }

        private void OnSearchChanged(string value)
        {
            _viewModel?.SetQuery(value);

            if (_clearButtonObject != null)
                _clearButtonObject.SetActive(!string.IsNullOrEmpty(value));
        }

        private void OnSearchFocused(string _) => _viewModel?.SetSearchFocused(true);
        private void OnSearchUnfocused(string _) => _viewModel?.SetSearchFocused(false);

        private void OnClearClicked()
        {
            if (_searchInput != null)
                _searchInput.text = string.Empty;
        }
    }
}

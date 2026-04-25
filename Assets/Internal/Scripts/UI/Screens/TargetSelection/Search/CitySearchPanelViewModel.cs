using System;
using System.Collections.Generic;
using Internal.Scripts.Camera;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Quests;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.UI.Tooltip;
using R3;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.TargetSelection.Search
{
    public sealed class CitySearchPanelViewModel
    {
        private readonly EconomyDatabase _economyDb;
        private readonly UiThemeService _themeService;
        private readonly BuildingFilterCatalog _buildingFilterCatalog;
        private readonly TooltipService _tooltipService;
        private readonly CameraController _cameraController;
        private readonly CityCardBuilder _cardBuilder;

        private readonly ReactiveProperty<CitySearchViewState> _state = new();
        private readonly ReactiveProperty<string> _searchQuery = new(string.Empty);
        private readonly ReactiveProperty<BuildingId?> _activeFilter = new(null);
        private readonly ReactiveProperty<bool> _isVisible = new(true);

        private IDisposable _searchSubscription;
        private IDisposable _filterSubscription;
        private bool _searchFocused;

        public event Action<CityData> CityRowClicked;

        public Observable<CitySearchViewState> State => _state;
        public Observable<bool> IsVisible => _isVisible;
        public UiThemeService ThemeService => _themeService;
        public BuildingFilterCatalog BuildingFilterCatalog => _buildingFilterCatalog;
        public TooltipService TooltipService => _tooltipService;

        public CitySearchPanelViewModel(
            EconomyDatabase economyDb,
            UiThemeService themeService,
            BuildingFilterCatalog buildingFilterCatalog,
            TooltipService tooltipService,
            CameraController cameraController,
            CityCardBuilder cardBuilder)
        {
            _economyDb = economyDb;
            _themeService = themeService;
            _buildingFilterCatalog = buildingFilterCatalog;
            _tooltipService = tooltipService;
            _cameraController = cameraController;
            _cardBuilder = cardBuilder;
        }

        public void Activate()
        {
            Deactivate();
            _searchQuery.Value = string.Empty;
            _activeFilter.Value = null;
            _isVisible.Value = true;

            _searchSubscription = _searchQuery.Subscribe(_ => RebuildState());
            _filterSubscription = _activeFilter.Subscribe(_ => RebuildState());
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        public void Deactivate()
        {
            if (_searchFocused)
            {
                _cameraController?.ResumeInput();
                _searchFocused = false;
            }

            _searchSubscription?.Dispose();
            _searchSubscription = null;
            _filterSubscription?.Dispose();
            _filterSubscription = null;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _state.Value = null;
        }

        public void SetQuery(string query)
        {
            _searchQuery.Value = query ?? string.Empty;
        }

        public void SetFilter(BuildingId? filter)
        {
            _activeFilter.Value = _activeFilter.Value == filter ? null : filter;
        }

        public void SetVisible(bool value)
        {
            _isVisible.Value = value;
        }

        public void SetSearchFocused(bool focused)
        {
            if (_searchFocused == focused)
                return;

            _searchFocused = focused;
            if (_cameraController == null)
                return;

            if (focused)
                _cameraController.SuspendInput();
            else
                _cameraController.ResumeInput();
        }

        public void NotifyCityClickedById(string cityId)
        {
            if (string.IsNullOrEmpty(cityId))
                return;

            var city = _economyDb.Cities.Find(c => c.Id == cityId);
            if (city != null)
                CityRowClicked?.Invoke(city);
        }

        private void OnLocaleChanged(UnityEngine.Localization.Locale _) => RebuildState();

        private void RebuildState()
        {
            string query = _searchQuery.Value ?? string.Empty;
            BuildingId? filter = _activeFilter.Value;

            bool hasSearch = !string.IsNullOrWhiteSpace(query) || filter.HasValue;
            if (!hasSearch)
            {
                _state.Value = new CitySearchViewState(null, filter);
                return;
            }

            var results = FilterCities(query, filter);
            _state.Value = new CitySearchViewState(results, filter);
        }

        private List<CityRowData> FilterCities(string query, BuildingId? filter)
        {
            var cities = _economyDb.Cities;
            var result = new List<CityRowData>(cities.Count);
            bool hasQuery = !string.IsNullOrWhiteSpace(query);

            foreach (var city in cities)
            {
                if (filter.HasValue && !city.HasBuilding(filter.Value))
                    continue;

                CityRowData row = _cardBuilder.Build(city);

                if (hasQuery && !string.IsNullOrEmpty(row.Name) &&
                    !row.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    continue;

                result.Add(row);
            }

            return result;
        }
    }
}

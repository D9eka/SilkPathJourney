using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.UI.Theme;
using R3;

namespace Internal.Scripts.UI.Screens.EnterCity
{
    public sealed class EnterCityScreenViewModel : ScreenViewModelBase
    {
        private readonly EconomyDatabase _database;
        private readonly ScreenStackService _screenStackService;
        private readonly UiThemeService _themeService;
        private readonly ReactiveProperty<EnterCityViewState> _state = new();

        private string _cityId;

        public UiThemeService ThemeService => _themeService;

        public EnterCityScreenViewModel(EconomyDatabase database, ScreenStackService screenStackService, UiThemeService themeService)
        {
            _database = database;
            _screenStackService = screenStackService;
            _themeService = themeService;
        }

        public override ScreenId Id => ScreenId.EnterCity;
        public Observable<EnterCityViewState> State => _state;

        protected override void OnOpen(object args)
        {
            _cityId = args as string;
            BuildState();
        }

        protected override void OnClose()
        {
            _state.Value = null;
        }

        public void OpenBuilding(BuildingEntry entry)
        {
            if (entry.HasInteraction)
                _screenStackService.TryOpen(entry.InteractionScreen, _cityId, out _);
        }

        private const string ContextCity = "EnterCity.City";
        private const string ContextBuilding = "EnterCity.Building";
        private const string ContextBuildingDesc = "EnterCity.Building.Desc";

        private void BuildState()
        {
            var city = _database.Cities.Find(c => string.Equals(c.Id, _cityId, StringComparison.OrdinalIgnoreCase));
            string cityName = city != null
                ? LocalizationService.ResolveString(city.Name, _cityId, ContextCity)
                : _cityId;

            var entries = new List<BuildingEntry>();

            if (city != null)
            {
                foreach (var buildingId in city.Buildings)
                {
                    var building = _database.GetBuilding(buildingId);
                    if (building == null || building.InteractionScreen == ScreenId.None)
                        continue;

                    string name = LocalizationService.ResolveString(building.Name, building.Type.ToString(), ContextBuilding);
                    string desc = LocalizationService.ResolveString(building.Description, string.Empty, ContextBuildingDesc);
                    entries.Add(new BuildingEntry(name, desc, building.InteractionScreen));
                }
            }

            _state.Value = new EnterCityViewState(cityName, entries);
        }
    }
}

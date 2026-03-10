using System.Collections.Generic;
using Internal.Scripts.UI.Screens.Core.Config;

namespace Internal.Scripts.UI.Screens.EnterCity
{
    public readonly struct BuildingEntry
    {
        public readonly string Name;
        public readonly string Description;
        public readonly ScreenId InteractionScreen;
        public readonly bool HasInteraction;

        public BuildingEntry(string name, string description, ScreenId interactionScreen)
        {
            Name = name;
            Description = description;
            InteractionScreen = interactionScreen;
            HasInteraction = interactionScreen != ScreenId.None;
        }
    }

    public sealed class EnterCityViewState
    {
        public readonly string CityName;
        public readonly IReadOnlyList<BuildingEntry> Buildings;

        public EnterCityViewState(string cityName, IReadOnlyList<BuildingEntry> buildings)
        {
            CityName = cityName;
            Buildings = buildings;
        }
    }
}

using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Quests;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.TargetSelection.Search
{
    public sealed class CityCardBuilder
    {
        private readonly EconomyDatabase _economyDb;
        private readonly BuildingFilterCatalog _buildingFilterCatalog;
        private readonly QuestCityIndicatorService _questIndicator;

        public CityCardBuilder(
            EconomyDatabase economyDb,
            BuildingFilterCatalog buildingFilterCatalog,
            QuestCityIndicatorService questIndicator)
        {
            _economyDb = economyDb;
            _buildingFilterCatalog = buildingFilterCatalog;
            _questIndicator = questIndicator;
        }

        public CityRowData Build(CityData city)
        {
            if (city == null)
                return default;

            string name = ResolveCityName(city);
            var typeData = _economyDb.GetCityType(city.Type);
            Sprite cityIcon = typeData?.Icon;
            IconLabelEntry[] buildingEntries = ResolveBuildingEntries(city);
            string questText = _questIndicator?.GetIndicatorText(city.Id);

            CitySpecializationVm specialization = CitySpecializationVm.Build(typeData);

            return new CityRowData(
                cityId: city.Id,
                nodeId: city.NodeId,
                name: name,
                cityIcon: cityIcon,
                buildingEntries: buildingEntries,
                cityTooltip: typeData,
                questIndicatorText: questText,
                specialization: specialization);
        }

        private IconLabelEntry[] ResolveBuildingEntries(CityData city)
        {
            if (city.Buildings == null || city.Buildings.Length == 0)
                return Array.Empty<IconLabelEntry>();

            var list = new List<IconLabelEntry>(city.Buildings.Length);
            foreach (BuildingId buildingId in city.Buildings)
            {
                var building = _economyDb.GetBuilding(buildingId);
                if (building == null) continue;

                string label = LocalizationService.ResolveString(
                    building.Name, building.Type.ToString(), $"CityCard.Building.{buildingId}");
                Sprite icon = _buildingFilterCatalog?.Get(buildingId)?.Icon;
                list.Add(new IconLabelEntry(icon, label, building));
            }
            return list.ToArray();
        }

        private static string ResolveCityName(CityData city)
        {
            return LocalizationService.ResolveString(city.Name, city.Id, $"CityCard.City.{city.Id}");
        }
    }
}

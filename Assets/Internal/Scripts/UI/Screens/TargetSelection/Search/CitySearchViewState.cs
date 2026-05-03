using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.TargetSelection.Search
{
    public sealed class CitySearchViewState
    {
        public readonly IReadOnlyList<CityRowData> Results;
        public readonly int ResultsCount;
        public readonly bool ShowResults;
        public readonly bool ShowEmpty;
        public readonly IReadOnlyCollection<BuildingId> ActiveFilters;
        public readonly bool QuestFilterActive;

        public CitySearchViewState(
            IReadOnlyList<CityRowData> results,
            IReadOnlyCollection<BuildingId> activeFilters,
            bool questFilterActive = false)
        {
            Results = results;
            ResultsCount = results?.Count ?? 0;
            ActiveFilters = activeFilters;
            QuestFilterActive = questFilterActive;
            ShowResults = results != null && results.Count > 0;
            ShowEmpty = results != null && results.Count == 0;
        }
    }

    public readonly struct CityRowData
    {
        public readonly string CityId;
        public readonly string NodeId;
        public readonly string Name;
        public readonly Sprite CityIcon;
        public readonly ITooltipDataProvider CityTooltip;
        public readonly IReadOnlyList<IconLabelEntry> BuildingEntries;
        public readonly string QuestIndicatorText;
        public readonly CitySpecializationVm Specialization;
        public bool HasQuestActivity => !string.IsNullOrEmpty(QuestIndicatorText);

        public CityRowData(
            string cityId,
            string nodeId,
            string name,
            Sprite cityIcon,
            IReadOnlyList<IconLabelEntry> buildingEntries,
            ITooltipDataProvider cityTooltip = null,
            string questIndicatorText = null,
            CitySpecializationVm specialization = null)
        {
            CityId = cityId;
            NodeId = nodeId;
            Name = name;
            CityIcon = cityIcon;
            CityTooltip = cityTooltip;
            BuildingEntries = buildingEntries;
            QuestIndicatorText = questIndicatorText;
            Specialization = specialization;
        }
    }
}

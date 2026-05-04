using System.Collections.Generic;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Tooltip;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.TargetSelection.Search
{
    public class CityInfoView : MonoBehaviour, ILocalizationConsumer
    {
        [Header("City")]
        [SerializeField] private IconLabel _cityNameView;

        [Header("Buildings")]
        [SerializeField] private TextMeshProUGUI _buildingsHeaderText;
        [SerializeField] private LocalizedString _buildingsHeaderLocalized;
        [SerializeField] private RectTransform _buildingsContent;
        [SerializeField] private IconLabel _iconLabelPrefab;

        [Header("Quest Marker")]
        [SerializeField] private IconLabel _questIndicatorView;

        private readonly List<IconLabel> _spawnedBuildings = new();
        private TooltipService _tooltipService;
        private LocalizationService _localization;
        private LocalizationService.LocalizedTextHandle _buildingsHeaderHandle;

        public void SetTooltipService(TooltipService service)
        {
            _tooltipService = service;
        }

        public void SetLocalization(LocalizationService localization)
        {
            _localization = localization;
            BindBuildingsHeader();
        }

        public void Apply(Sprite icon, string label, ITooltipDataProvider cityTooltip,
            IReadOnlyList<IconLabelEntry> buildings, string questIndicatorText)
        {
            if (_cityNameView != null)
            {
                _cityNameView.Initialize(icon, label);
                _cityNameView.SetTooltip(cityTooltip, _tooltipService);
            }

            UpdateBuildingsHeaderVisibility(buildings);
            RebuildBuildings(buildings);
            ApplyQuestIndicator(questIndicatorText);
        }

        private void BindBuildingsHeader()
        {
            _buildingsHeaderHandle?.Dispose();
            if (_buildingsHeaderText == null || _buildingsHeaderLocalized == null || _localization == null)
                return;
            _buildingsHeaderHandle = _localization.BindText(
                _buildingsHeaderText, _buildingsHeaderLocalized, "CityInfo.BuildingsLabel");
        }

        private void UpdateBuildingsHeaderVisibility(IReadOnlyList<IconLabelEntry> buildings)
        {
            if (_buildingsHeaderText == null) return;
            bool hasBuildings = buildings != null && buildings.Count > 0;
            _buildingsHeaderText.gameObject.SetActive(hasBuildings);
        }

        private void ApplyQuestIndicator(string questText)
        {
            if (_questIndicatorView == null)
                return;

            bool show = !string.IsNullOrEmpty(questText);
            _questIndicatorView.gameObject.SetActive(show);
            if (show)
                _questIndicatorView.SetLabel(questText);
        }

        private void RebuildBuildings(IReadOnlyList<IconLabelEntry> entries)
        {
            foreach (IconLabel view in _spawnedBuildings)
            {
                if (view != null)
                {
                    view.ClearTooltip();
                    Destroy(view.gameObject);
                }
            }
            _spawnedBuildings.Clear();

            if (entries == null || _iconLabelPrefab == null || _buildingsContent == null)
                return;

            foreach (IconLabelEntry entry in entries)
            {
                IconLabel view = Instantiate(_iconLabelPrefab, _buildingsContent);
                view.Initialize(entry.Icon, entry.Label);
                if (entry.TooltipProvider != null)
                    view.SetTooltip(entry.TooltipProvider, _tooltipService);
                _spawnedBuildings.Add(view);
            }
        }

        private void OnDestroy()
        {
            _buildingsHeaderHandle?.Dispose();
            _buildingsHeaderHandle = null;
        }
    }
}

using System;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.TargetSelection.Search
{
    [RequireComponent(typeof(IconFilterButton))]
    public class BuildingFilterButton : MonoBehaviour
    {
        [SerializeField] private BuildingId _building;

        private IconFilterButton _iconButton;

        public BuildingId Building => _building;

        private void EnsureButton()
        {
            if (_iconButton == null)
                _iconButton = GetComponent<IconFilterButton>();
        }

        public void Initialize(BuildingFilterCatalog catalog, TooltipService tooltipService, Action onClick)
        {
            EnsureButton();

            BuildingFilterEntry entry = catalog?.Get(_building);
            Sprite icon = entry?.Icon;
            string tooltip = entry?.Name != null && !entry.Name.IsEmpty
                ? LocalizationService.ResolveString(entry.Name, _building.ToString(), $"Building.{_building}.Name")
                : LocalizationService.Resolve("Economy",
                    $"building.{_building.ToString().ToLowerInvariant()}.name", _building.ToString());

            _iconButton.Configure(icon, tooltip, tooltipService, onClick);
        }

        public void InitializeWithTooltip(BuildingFilterCatalog catalog, TooltipService tooltipService,
            string tooltip, Action onClick)
        {
            EnsureButton();

            BuildingFilterEntry entry = catalog?.Get(_building);
            Sprite icon = entry?.Icon;
            _iconButton.Configure(icon, tooltip, tooltipService, onClick);
        }

        public void InitializeWithSprite(Sprite icon, string tooltip,
            TooltipService tooltipService, Action onClick)
        {
            EnsureButton();
            _iconButton.Configure(icon, tooltip, tooltipService, onClick);
        }

        public void SetActive(bool active)
        {
            EnsureButton();
            _iconButton.SetActive(active);
        }

        public void Unbind()
        {
            if (_iconButton != null)
                _iconButton.Unbind();
        }
    }
}

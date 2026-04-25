using System;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Trade
{
    [RequireComponent(typeof(IconFilterButton))]
    public class CategoryFilterButton : MonoBehaviour
    {
        [SerializeField] private ItemType _category;

        private IconFilterButton _iconButton;

        public ItemType Category => _category;

        public void Initialize(ItemCategoryCatalog catalog, TooltipService tooltipService, Action onClick)
        {
            if (_iconButton == null)
                _iconButton = GetComponent<IconFilterButton>();

            ItemCategoryEntry entry = catalog?.Get(_category);
            Sprite icon = entry?.Icon;
            string tooltip = entry?.Name != null && !entry.Name.IsEmpty
                ? LocalizationService.ResolveString(entry.Name, _category.ToString(), $"ItemCategory.{_category}.Name")
                : LocalizationService.Resolve("Economy",
                    $"item_category.{_category.ToString().ToLowerInvariant()}.name", _category.ToString());

            _iconButton.Configure(icon, tooltip, tooltipService, onClick);
        }

        public void SetActive(bool active)
        {
            if (_iconButton == null)
                _iconButton = GetComponent<IconFilterButton>();
            _iconButton.SetActive(active);
        }

        public void Unbind()
        {
            if (_iconButton != null)
                _iconButton.Unbind();
        }
    }
}

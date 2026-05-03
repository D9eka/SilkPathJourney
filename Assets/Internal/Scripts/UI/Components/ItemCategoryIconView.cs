using Internal.Scripts.Economy.Generated;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Internal.Scripts.UI.Components
{
    public class ItemCategoryIconView : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private ItemType _itemType;

        [Inject(Optional = true)] private ItemCategoryCatalog _catalog;
        [Inject(Optional = true)] private TooltipService _tooltipService;

        private HoverReporter _hover;
        private System.Action _hoverEnter;
        private System.Action _hoverExit;
        private CategoryTooltipProvider _tooltipProvider;

        private void Awake()
        {
            ApplyFromCatalog();
        }

        public void SetSprite(Sprite sprite)
        {
            _image.sprite = sprite;
        }

        public void SetItemType(ItemType type)
        {
            _itemType = type;
            ApplyFromCatalog();
            UpdateTooltip();
        }

        private void ApplyFromCatalog()
        {
            if (_catalog == null) return;
            ItemCategoryEntry entry = _catalog.Get(_itemType);
            if (entry != null && entry.Icon != null) _image.sprite = entry.Icon;
        }

        private void UpdateTooltip()
        {
            if (_tooltipService == null || _catalog == null) return;

            ItemCategoryEntry entry = _catalog.Get(_itemType);
            if (entry == null) return;

            _tooltipProvider = new CategoryTooltipProvider(entry, _itemType);

            if (_hover == null)
            {
                _hover = HoverReporter.GetOrAdd(gameObject);
                _hoverEnter = () => _tooltipService.ShowTooltipDelayed(_tooltipProvider);
                _hoverExit = () => _tooltipService.HideTooltip();
                _hover.Entered += _hoverEnter;
                _hover.Exited += _hoverExit;
            }
        }

        private void OnDisable()
        {
            if (_hover != null)
            {
                if (_hoverEnter != null) _hover.Entered -= _hoverEnter;
                if (_hoverExit != null) _hover.Exited -= _hoverExit;
                _hover = null;
                _hoverEnter = null;
                _hoverExit = null;
            }
        }

        private sealed class CategoryTooltipProvider : ITooltipDataProvider
        {
            private readonly ItemCategoryEntry _entry;
            private readonly ItemType _type;

            public CategoryTooltipProvider(ItemCategoryEntry entry, ItemType type)
            {
                _entry = entry;
                _type = type;
            }

            public string GetTooltipTitle()
            {
                if (_entry?.Name != null)
                    return LocalizationService.ResolveString(_entry.Name, _type.ToString(), $"ItemCategory.{_type}");
                return _type.ToString();
            }

            public string GetTooltipDescription() => string.Empty;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Items;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Inventory;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Trade
{
    public class TradeContainer : MonoBehaviour, ILocalizationConsumer
    {
        [Header("Components")]
        [SerializeField] private HeaderElement _mainHeader;
        [SerializeField] private ResourceIndicator _totalIndicator;
        [SerializeField] private bool _increaseIsPositive;

        [Header("Content")]
        [SerializeField] private ItemsView _itemsView;

        [Header("Filter")]
        [SerializeField] private CategoryFilterButton[] _filterButtons;

        [Header("Localization")]
        [SerializeField] private LocalizedString _mainHeaderLocalizedString;

        private LocalizationService _localization;
        private LocalizationService.LocalizedTextHandle _headerHandle;

        private readonly HashSet<ItemType> _activeFilter = new();
        private IReadOnlyList<ItemRowData> _allItems;
        private bool _showWeight;
        private bool _showPrice;
        private TooltipService _tooltipService;
        private ItemCategoryCatalog _categoryCatalog;
        private bool _buttonsBound;

        public ItemsView ItemsView => _itemsView;
        public HeaderElement MainHeader => _mainHeader;

        public void SetLocalization(LocalizationService service)
        {
            _localization = service;
            BindHeaderLocalization();
        }

        public void SetServices(TooltipService tooltip, ItemCategoryCatalog categoryCatalog)
        {
            _tooltipService = tooltip;
            _categoryCatalog = categoryCatalog;
            BindFilterButtons();
            UpdateFilterVisuals();
        }

        private void OnEnable()
        {
            if (_localization != null)
                BindHeaderLocalization();
        }

        private void OnDisable()
        {
            _headerHandle?.Dispose();
            _headerHandle = null;
            UnbindFilterButtons();
        }

        private void BindHeaderLocalization()
        {
            _headerHandle?.Dispose();
            if (_mainHeader != null && _mainHeader.Text != null && _mainHeaderLocalizedString != null)
                _headerHandle = _localization.BindText(_mainHeader.Text, _mainHeaderLocalizedString, $"{name}.MainHeader");
        }

        public void SetMainHeaderText(string value) =>
            _mainHeader?.SetText(value);

        public void SetTotalIcon(Sprite icon) =>
            _totalIndicator?.SetIcon(icon);

        public void SetTotal(int value) =>
            _totalIndicator?.SetValue(value);

        public void SetTotalChange(int change) =>
            _totalIndicator?.SetChange(change, _increaseIsPositive);

        public void SetTotalHighlight(bool warning) =>
            _totalIndicator?.SetHighlight(warning);

        public void SetItems(IReadOnlyList<ItemRowData> items, bool showWeight, bool showPrice)
        {
            _allItems = items;
            _showWeight = showWeight;
            _showPrice = showPrice;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_itemsView == null)
                return;

            IReadOnlyList<ItemRowData> effective = _allItems;
            if (_allItems != null && _activeFilter.Count > 0)
                effective = _allItems.Where(r => _activeFilter.Contains(r.Category)).ToList();

            _itemsView.SetItems(effective, _showWeight, _showPrice);
        }

        private void BindFilterButtons()
        {
            if (_buttonsBound || _filterButtons == null || _filterButtons.Length == 0)
                return;

            foreach (CategoryFilterButton btn in _filterButtons)
            {
                if (btn == null)
                    continue;

                ItemType cat = btn.Category;
                btn.Initialize(_categoryCatalog, _tooltipService, () => ToggleFilter(cat));
            }

            _buttonsBound = true;
        }

        private void UnbindFilterButtons()
        {
            if (!_buttonsBound || _filterButtons == null)
                return;

            foreach (CategoryFilterButton btn in _filterButtons)
                btn?.Unbind();

            _buttonsBound = false;
        }

        private void ToggleFilter(ItemType category)
        {
            if (!_activeFilter.Remove(category))
                _activeFilter.Add(category);

            UpdateFilterVisuals();
            ApplyFilter();
        }

        private void UpdateFilterVisuals()
        {
            if (_filterButtons == null)
                return;

            foreach (CategoryFilterButton btn in _filterButtons)
            {
                if (btn == null)
                    continue;
                btn.SetActive(_activeFilter.Contains(btn.Category));
            }
        }
    }
}

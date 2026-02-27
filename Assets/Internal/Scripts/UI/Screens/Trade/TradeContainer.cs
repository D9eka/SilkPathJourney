using System.Collections.Generic;
using Internal.Scripts.Items;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Inventory;
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

        [Header("Localization")]
        [SerializeField] private LocalizedString _mainHeaderLocalizedString;

        private LocalizationService _localization;
        private LocalizationService.LocalizedTextHandle _headerHandle;

        public ItemsView ItemsView => _itemsView;
        public HeaderElement MainHeader => _mainHeader;

        public void SetLocalization(LocalizationService service)
        {
            _localization = service;
            BindHeaderLocalization();
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

        public void SetItems(IReadOnlyList<ItemRowData> items, bool showWeight, bool showPrice) =>
            _itemsView.SetItems(items, showWeight, showPrice);
    }
}

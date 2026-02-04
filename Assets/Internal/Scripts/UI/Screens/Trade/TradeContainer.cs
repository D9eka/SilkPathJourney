using System.Collections.Generic;
using Internal.Scripts.Items;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Trade
{
    public class TradeContainer : MonoBehaviour
    {
        [Header("Headers")]
        [SerializeField] private GameObject _mainHeader;
        [SerializeField] private GameObject _additionalHeader;
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _mainHeaderText;
        [SerializeField] private TextMeshProUGUI _additionalHeaderText;
        [Header("Content")]
        [SerializeField] private ItemsView _itemsView;
        [Header("LocalizedStrings")]
        [SerializeField] private LocalizedString _mainHeaderLocalizedString;
        [SerializeField] private LocalizedString _additionalHeaderLocalizedString;
        [Header("Colors")]
        [SerializeField] private Color _additionalHeaderWarningColor = new Color(1f, 0.3f, 0.3f, 1f);
        private Color _additionalHeaderNormalColor;
        private bool _cachedHeaderColor;
        private LocalizationHelper.LocalizedTextHandle _mainHeaderHandle;
        private LocalizationHelper.LocalizedTextHandle _additionalHeaderHandle;
        private string _mainHeaderValue = string.Empty;
        private string _additionalHeaderValue = string.Empty;
        private object[] _mainHeaderArgs;
        private object[] _additionalHeaderArgs;

        public ItemsView ItemsView => _itemsView;

        private void OnEnable()
        {
            CacheHeaderColors();
            BindLocalization();
        }

        private void OnDisable()
        {
            _mainHeaderHandle?.Dispose();
            _mainHeaderHandle = null;
            _additionalHeaderHandle?.Dispose();
            _additionalHeaderHandle = null;
        }

        public void SetMainHeaderText(string value, params object[] args)
        {
            _mainHeaderValue = value ?? string.Empty;
            _mainHeaderArgs = args != null && args.Length > 0
                ? args
                : new object[] { _mainHeaderValue };
            if (_mainHeaderHandle != null)
                _mainHeaderHandle.SetArguments(_mainHeaderValue, _mainHeaderArgs);
            else
                _mainHeaderText.text = _mainHeaderValue;
        }

        public void SetAdditionalHeaderText(string value, params object[] args)
        {
            _additionalHeaderValue = value ?? string.Empty;
            _additionalHeaderArgs = args != null && args.Length > 0
                ? args
                : new object[] { _additionalHeaderValue };
            if (_additionalHeaderHandle != null)
                _additionalHeaderHandle.SetArguments(_additionalHeaderValue, _additionalHeaderArgs);
            else
                _additionalHeaderText.text = _additionalHeaderValue;
        }

        public void SetAdditionalHeaderHighlight(bool state)
        {
            CacheHeaderColors();
            _additionalHeaderText.color = state ? _additionalHeaderWarningColor : _additionalHeaderNormalColor;
        }

        public void SetItems(IReadOnlyList<ItemRowData> items, bool showWeight, bool showPrice)
        {
            _itemsView.SetItems(items, showWeight, showPrice);
        }

        private void BindLocalization()
        {
            _mainHeaderHandle?.Dispose();
            _additionalHeaderHandle?.Dispose();
            _mainHeaderHandle = LocalizationHelper.BindText(_mainHeaderText, _mainHeaderLocalizedString, $"{name}.MainHeader");
            _additionalHeaderHandle = LocalizationHelper.BindText(_additionalHeaderText, _additionalHeaderLocalizedString, $"{name}.AdditionalHeader");
            if (!string.IsNullOrWhiteSpace(_mainHeaderValue))
                _mainHeaderHandle.SetArguments(
                    _mainHeaderValue,
                    _mainHeaderArgs ?? new object[] { _mainHeaderValue });
            if (!string.IsNullOrWhiteSpace(_additionalHeaderValue))
                _additionalHeaderHandle.SetArguments(
                    _additionalHeaderValue,
                    _additionalHeaderArgs ?? new object[] { _additionalHeaderValue });
        }

        private void CacheHeaderColors()
        {
            if (_cachedHeaderColor)
                return;

            _additionalHeaderNormalColor = _additionalHeaderText.color;
            _cachedHeaderColor = true;
        }
    }
}

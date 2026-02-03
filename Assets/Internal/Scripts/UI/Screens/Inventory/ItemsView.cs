using System;
using System.Collections.Generic;
using Internal.Scripts.Items;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Internal.Scripts.UI.Screens.Inventory
{
    public class ItemsView : MonoBehaviour
    {

        [Header("Headers")]
        [SerializeField] private GameObject _itemsNameHeader;
        [SerializeField] private GameObject _itemsWeightHeader;
        [SerializeField] private GameObject _itemsPriceHeader;
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _itemsNameHeaderText;
        [SerializeField] private TextMeshProUGUI _itemsWeightHeaderText;
        [SerializeField] private TextMeshProUGUI _itemsPriceHeaderText;
        [SerializeField] private TextMeshProUGUI _actionButtonText;
        [Header("Buttons")]
        [SerializeField] private Button _actionButton;
        [Header("Content")]
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private ItemView _itemPrefab;
        [Header("LocalizedStrings")]
        [SerializeField] private LocalizedString _itemsNameHeaderLocalizedString;
        [SerializeField] private LocalizedString _itemsWeightHeaderLocalizedString;
        [SerializeField] private LocalizedString _itemsPriceHeaderLocalizedString;
        [SerializeField] private LocalizedString _actionButtonLocalizedString;

        private readonly List<ItemView> _spawnedItems = new();
        private readonly List<ItemRowData> _items = new();
        private int _selectedIndex = -1;
        private Vector2 _lastNavigateValue = Vector2.zero;
        private UnityAction _actionHandler;
        
        private LocalizationHelper.LocalizedTextHandle _nameHeaderHandle;
        private LocalizationHelper.LocalizedTextHandle _weightHeaderHandle;
        private LocalizationHelper.LocalizedTextHandle _priceHeaderHandle;
        private LocalizationHelper.LocalizedTextHandle _actionButtonHandle;

        public event Action<ItemRowData> ItemSelected;
        public event Action<ItemRowData, bool> ItemActivated;

        private void OnEnable()
        {
            BindLocalization();
        }

        private void OnDisable()
        {
            UnbindAction();
            _nameHeaderHandle?.Dispose();
            _nameHeaderHandle = null;
            _weightHeaderHandle?.Dispose();
            _weightHeaderHandle = null;
            _priceHeaderHandle?.Dispose();
            _priceHeaderHandle = null;
            _actionButtonHandle?.Dispose();
            _actionButtonHandle = null;
        }

        public void SetWeightHeaderState(bool state)
        {
            _itemsWeightHeader.SetActive(state);
        }

        public void SetPriceHeaderState(bool state)
        {
            _itemsPriceHeader.SetActive(state);
        }

        public void SetItems(IReadOnlyList<ItemRowData> items, bool showWeight, bool showPrice)
        {
            SetWeightHeaderState(showWeight);
            SetPriceHeaderState(showPrice);

            _items.Clear();
            if (items != null)
            {
                for (int i = 0; i < items.Count; i++)
                    _items.Add(items[i]);
            }

            int count = _items.Count;
            EnsurePool(count);

            for (int i = 0; i < count; i++)
            {
                ItemRowData data = _items[i];
                ItemView view = _spawnedItems[i];
                view.gameObject.SetActive(true);
                view.Bind(this, i);
                view.SetWeightState(showWeight);
                view.SetPriceState(showPrice);
                view.SetData(data.Name, data.Weight, data.Price);
            }

            for (int i = count; i < _spawnedItems.Count; i++)
                _spawnedItems[i].gameObject.SetActive(false);

            if (_selectedIndex >= count)
                _selectedIndex = count - 1;

            UpdateSelectionVisual();
        }

        public void BindAction(Action action)
        {
            UnbindAction();
            if (action == null)
                return;

            _actionHandler = () => action.Invoke();
            _actionButton.onClick.AddListener(_actionHandler);
        }

        public void UnbindAction()
        {
            if (_actionHandler == null)
                return;

            _actionButton.onClick.RemoveListener(_actionHandler);
            _actionHandler = null;
        }

        public bool TryActivateSelected(bool addAll)
        {
            if (_items.Count == 0)
                return false;

            if (_selectedIndex < 0)
                SelectIndex(0);

            if (_selectedIndex >= _items.Count)
                return false;

            ActivateIndex(_selectedIndex, addAll);
            return true;
        }

        public void ClearSelection()
        {
            _selectedIndex = -1;
            UpdateSelectionVisual();
        }

        public bool TryGetSelected(out ItemRowData item)
        {
            if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
            {
                item = _items[_selectedIndex];
                return true;
            }

            item = default;
            return false;
        }

        public void EnsureSelection()
        {
            if (_selectedIndex < 0 && _items.Count > 0)
                SelectIndex(0);
        }

        public void HandleNavigate(Vector2 value)
        {
            float y = value.y;
            if (Mathf.Abs(y) < 0.5f)
            {
                _lastNavigateValue = value;
                return;
            }

            if (Mathf.Abs(_lastNavigateValue.y) >= 0.5f)
                return;

            int delta = y > 0f ? -1 : 1;
            SelectIndex(_selectedIndex + delta);
            _lastNavigateValue = value;
        }

        internal void HandleItemClick(int index, bool isDoubleClick, bool addAll)
        {
            SelectIndex(index);
            if (isDoubleClick)
                ActivateIndex(index, addAll);
        }

        private void SelectIndex(int index)
        {
            if (_items.Count == 0)
            {
                _selectedIndex = -1;
                UpdateSelectionVisual();
                return;
            }

            int clamped = Mathf.Clamp(index, 0, _items.Count - 1);
            if (_selectedIndex == clamped)
            {
                ItemSelected?.Invoke(_items[_selectedIndex]);
                return;
            }

            _selectedIndex = clamped;
            UpdateSelectionVisual();
            ItemSelected?.Invoke(_items[_selectedIndex]);
        }

        private void UpdateSelectionVisual()
        {
            for (int i = 0; i < _spawnedItems.Count; i++)
            {
                bool selected = i == _selectedIndex && i < _items.Count;
                _spawnedItems[i].SetSelectionState(selected);
            }
        }

        private void ActivateIndex(int index, bool addAll)
        {
            if (index < 0 || index >= _items.Count)
                return;

            ItemActivated?.Invoke(_items[index], addAll);
        }

        private void EnsurePool(int count)
        {
            while (_spawnedItems.Count < count)
            {
                ItemView view = Instantiate(_itemPrefab, _itemsContainer);
                _spawnedItems.Add(view);
            }
        }

        private void BindLocalization()
        {
            _nameHeaderHandle?.Dispose();
            _weightHeaderHandle?.Dispose();
            _priceHeaderHandle?.Dispose();
            _actionButtonHandle?.Dispose();
            _nameHeaderHandle = LocalizationHelper.BindText(_itemsNameHeaderText, _itemsNameHeaderLocalizedString, $"{name}.NameHeader");
            _weightHeaderHandle = LocalizationHelper.BindText(_itemsWeightHeaderText, _itemsWeightHeaderLocalizedString, $"{name}.WeightHeader");
            _priceHeaderHandle = LocalizationHelper.BindText(_itemsPriceHeaderText, _itemsPriceHeaderLocalizedString, $"{name}.PriceHeader");
            _actionButtonHandle = LocalizationHelper.BindText(_actionButtonText, _actionButtonLocalizedString, $"{name}.ActionButton");
        }
    }
}

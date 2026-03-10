using Internal.Scripts.UI.Tooltip;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Inventory
{
    public class ItemView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _weightText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [Header("Content")]
        [SerializeField] private GameObject _name;
        [SerializeField] private GameObject _weightSeparator;
        [SerializeField] private GameObject _weight;
        [SerializeField] private GameObject _priceSeparator;
        [SerializeField] private GameObject _price;
        [SerializeField] private GameObject _itemSelectionBorder;

        private ItemsView _owner;
        private int _index;
        private TooltipService _tooltipService;
        private string _tooltipTitle;
        private string _tooltipText;

        public void Bind(ItemsView owner, int index)
        {
            _owner = owner;
            _index = index;
        }

        public void SetWeightState(bool state)
        {
            SetFieldState(_weight, _weightSeparator, state);
        }

        public void SetPriceState(bool state)
        {
            SetFieldState(_price, _priceSeparator, state);
        }

        public void SetSelectionState(bool state)
        {
            _itemSelectionBorder.SetActive(state);
        }

        public void SetTooltipService(TooltipService service)
        {
            _tooltipService = service;
        }

        public void SetData(string name, string weight, string price,
            string tooltipTitle = null, string tooltipText = null)
        {
            _nameText.text = name;
            _weightText.text = weight;
            _priceText.text = price;
            _tooltipTitle = tooltipTitle;
            _tooltipText = tooltipText;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
                return;

            bool isDoubleClick = eventData.clickCount >= 2;
            bool shift = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
            _owner?.HandleItemClick(_index, isDoubleClick, shift);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tooltipService != null && _tooltipText != null)
                _tooltipService.ShowTooltipDelayed(new SimpleTooltipData(_tooltipTitle, _tooltipText));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _tooltipService?.HideTooltip();
        }

        private void SetFieldState(GameObject field, GameObject separator, bool state)
        {
            field.SetActive(state);
            separator.SetActive(state);
        }
    }
}

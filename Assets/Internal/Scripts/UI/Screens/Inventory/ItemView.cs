using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Inventory
{
    public class ItemView : MonoBehaviour
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

        private void SetFieldState(GameObject field, GameObject separator, bool state)
        {
            field.SetActive(state);
            separator.SetActive(state);
        }
    }
}
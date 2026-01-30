using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

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

        public void SetWeightHeaderState(bool state)
        {
            _itemsWeightHeader.SetActive(state);
        }
    }
}
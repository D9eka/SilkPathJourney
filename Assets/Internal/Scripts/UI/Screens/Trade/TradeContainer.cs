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
    }
}
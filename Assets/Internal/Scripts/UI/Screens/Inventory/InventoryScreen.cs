using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Inventory
{
    public class InventoryScreen : PopupScreen
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _actionButtonText;
        [Header("Buttons")]
        [SerializeField] private Button _actionButton;
        [Header("Content")]
        [SerializeField] private ItemsView _itemsView;
        [Header("LocalizedStrings")]
        [SerializeField] private LocalizedString _actionButtonLocalizedString;
    }
}
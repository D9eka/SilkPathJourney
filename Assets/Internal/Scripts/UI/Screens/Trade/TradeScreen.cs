using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Trade
{
    public class TradeScreen : PopupScreen
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _tradeButtonText;
        [Header("Buttons")]
        [SerializeField] private Button _tradeButton;
        [Header("Content")]
        [SerializeField] private TradeContainer _playerTradeContainer;
        [SerializeField] private TradeContainer _itemsToBuyTradeContainer;
        [SerializeField] private TradeContainer _itemsToSellTradeContainer;
        [SerializeField] private TradeContainer _npcTradeContainer;
        [Header("LocalizedStrings")]
        [SerializeField] private LocalizedString _tradeButtonLocalizedString;
    }
}
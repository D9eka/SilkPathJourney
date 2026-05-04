using Internal.Scripts.Meta;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.RunEnd.Stats
{
    public sealed class EconomyStatsView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _headerText;

        [SerializeField] private TextMeshProUGUI _earnedText;
        [SerializeField] private TextMeshProUGUI _spentText;
        [SerializeField] private TextMeshProUGUI _netProfitText;
        [SerializeField] private TextMeshProUGUI _itemsBoughtText;
        [SerializeField] private TextMeshProUGUI _itemsSoldText;
        [SerializeField] private TextMeshProUGUI _bestDealText;

        private LocalizationService.LocalizedTextHandle _headerHandle;

        public void BindLocalization(LocalizationService localization)
        {
            _headerHandle?.Dispose();
            _headerHandle = localization.BindText(_headerText, "UI", LocUI.UI_RunEnd_Section_Economy, $"{name}.Header");
        }

        public void DisposeBinding()
        {
            _headerHandle?.Dispose();
            _headerHandle = null;
        }

        public void Apply(RunStatsData stats, string bestDealName)
        {
            _earnedText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_MoneyEarned, null, stats.MoneyEarned);
            _spentText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_MoneySpent, null, stats.MoneySpent);
            
            int net = stats.MoneyEarned - stats.MoneySpent;
            _netProfitText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_NetProfit, null, net);

            _itemsBoughtText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_ItemsBought, null, stats.ItemsBought);
            _itemsSoldText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_ItemsSold, null, stats.ItemsSold);
            _bestDealText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_BestDeal, null, bestDealName);
        }
    }
}

using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.LifetimeStats.Sections
{
    public class EconomyLifeStatsSection : LifeStatsSection
    {
        [SerializeField] private TextMeshProUGUI _earnedText;
        [SerializeField] private TextMeshProUGUI _spentText;
        [SerializeField] private TextMeshProUGUI _netProfitText;
        [SerializeField] private TextMeshProUGUI _itemsBoughtText;
        [SerializeField] private TextMeshProUGUI _itemsSoldText;
        [SerializeField] private TextMeshProUGUI _bestDealText;
        [SerializeField] private TextMeshProUGUI _favoriteItemText;
        [SerializeField] private TextMeshProUGUI _mostExpensiveItemText;
        [SerializeField] private TextMeshProUGUI _legacyEarnedTotalText;
        
        private LocalizationService.LocalizedTextHandle _earnedHandle;
        private LocalizationService.LocalizedTextHandle _spentHandle;
        private LocalizationService.LocalizedTextHandle _netProfitHandle;
        private LocalizationService.LocalizedTextHandle _itemsBoughtHandle;
        private LocalizationService.LocalizedTextHandle _itemsSoldHandle;
        private LocalizationService.LocalizedTextHandle _bestDealHandle;
        private LocalizationService.LocalizedTextHandle _favoriteItemHandle;
        private LocalizationService.LocalizedTextHandle _mostExpensiveHandle;
        private LocalizationService.LocalizedTextHandle _legacyEarnedHandle;
        
        public override void BindLocalization(LocalizationService localization)
        {
            DisposeBinding();
            HeaderHandle = BindValue(_header, LocUI.UI_LegacyShop_Lifetime_Section_Economy, localization);
            _earnedHandle = BindValue(_earnedText, LocUI.UI_LegacyShop_Lifetime_Earned, localization);
            _spentHandle = BindValue(_spentText, LocUI.UI_LegacyShop_Lifetime_Spent, localization);
            _netProfitHandle = BindValue(_netProfitText, LocUI.UI_LegacyShop_Lifetime_NetProfit, localization);
            _itemsBoughtHandle = BindValue(_itemsBoughtText, LocUI.UI_LegacyShop_Lifetime_ItemsBought, localization);
            _itemsSoldHandle = BindValue(_itemsSoldText, LocUI.UI_LegacyShop_Lifetime_ItemsSold, localization);
            _bestDealHandle = BindValue(_bestDealText, LocUI.UI_LegacyShop_Lifetime_BestDeal, localization);
            _favoriteItemHandle = BindValue(_favoriteItemText, LocUI.UI_LegacyShop_Lifetime_FavoriteItem, localization);
            _mostExpensiveHandle = BindValue(_mostExpensiveItemText, LocUI.UI_LegacyShop_Lifetime_MostExpensive, localization);
            _legacyEarnedHandle = BindValue(_legacyEarnedTotalText, LocUI.UI_LegacyShop_Lifetime_LegacyEarned, localization);
        }

        public override void DisposeBinding()
        {
            base.DisposeBinding();
            _earnedHandle?.Dispose(); _earnedHandle = null;
            _spentHandle?.Dispose(); _spentHandle = null;
            _netProfitHandle?.Dispose(); _netProfitHandle = null;
            _itemsBoughtHandle?.Dispose(); _itemsBoughtHandle = null;
            _itemsSoldHandle?.Dispose(); _itemsSoldHandle = null;
            _bestDealHandle?.Dispose(); _bestDealHandle = null;
            _favoriteItemHandle?.Dispose(); _favoriteItemHandle = null;
            _mostExpensiveHandle?.Dispose(); _mostExpensiveHandle = null;
            _legacyEarnedHandle?.Dispose(); _legacyEarnedHandle = null;
        }

        public override void Apply(LifetimeStatsViewState state)
        {
            var data = state.Raw;
            int netProfit = data.TotalMoneyEarned - data.TotalMoneySpent;
            SetArg(_earnedHandle, data.TotalMoneyEarned);
            SetArg(_spentHandle, data.TotalMoneySpent);
            SetArg(_netProfitHandle, netProfit);
            SetArg(_itemsBoughtHandle, data.TotalItemsBought);
            SetArg(_itemsSoldHandle, data.TotalItemsSold);
            SetArg(_bestDealHandle, state.BestDealText);
            SetArg(_favoriteItemHandle, state.FavoriteItemText);
            SetArg(_mostExpensiveHandle, state.MostExpensiveItemText);
            SetArg(_legacyEarnedHandle, data.TotalLegacyEarned);
        }
    }
}
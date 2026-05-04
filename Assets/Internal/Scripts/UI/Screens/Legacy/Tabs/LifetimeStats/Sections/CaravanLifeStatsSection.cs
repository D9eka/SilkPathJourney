using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.LifetimeStats.Sections
{
    public class CaravanLifeStatsSection : LifeStatsSection
    {
        [SerializeField] private TextMeshProUGUI _companionsHiredText;
        [SerializeField] private TextMeshProUGUI _companionsLostText;
        [SerializeField] private TextMeshProUGUI _companionsFinalText;
        [SerializeField] private TextMeshProUGUI _cartsText;
        [SerializeField] private TextMeshProUGUI _repairsText;
        [SerializeField] private TextMeshProUGUI _animalsLostText;
        [SerializeField] private TextMeshProUGUI _peakLoadText;
        
        private LocalizationService.LocalizedTextHandle _companionsHiredHandle;
        private LocalizationService.LocalizedTextHandle _companionsLostHandle;
        private LocalizationService.LocalizedTextHandle _companionsFinalHandle;
        private LocalizationService.LocalizedTextHandle _cartsHandle;
        private LocalizationService.LocalizedTextHandle _repairsHandle;
        private LocalizationService.LocalizedTextHandle _animalsLostHandle;
        private LocalizationService.LocalizedTextHandle _peakLoadHandle;
        
        public override void BindLocalization(LocalizationService localization)
        {
            base.BindLocalization(localization);
            HeaderHandle = BindValue(_header, LocUI.UI_LegacyShop_Lifetime_Section_Caravan, localization);
            _companionsHiredHandle = BindValue(_companionsHiredText, LocUI.UI_LegacyShop_Lifetime_CompanionsHired, localization);
            _companionsLostHandle = BindValue(_companionsLostText, LocUI.UI_LegacyShop_Lifetime_CompanionsLost, localization);
            _companionsFinalHandle = BindValue(_companionsFinalText, LocUI.UI_LegacyShop_Lifetime_CompanionsFinal, localization);
            _cartsHandle = BindValue(_cartsText, LocUI.UI_LegacyShop_Lifetime_Carts, localization);
            _repairsHandle = BindValue(_repairsText, LocUI.UI_LegacyShop_Lifetime_Repairs, localization);
            _animalsLostHandle = BindValue(_animalsLostText, LocUI.UI_LegacyShop_Lifetime_AnimalsLost, localization);
            _peakLoadHandle = BindValue(_peakLoadText, LocUI.UI_LegacyShop_Lifetime_PeakLoad, localization);

        }

        public override void DisposeBinding()
        {
            base.DisposeBinding();
            _companionsHiredHandle?.Dispose(); _companionsHiredHandle = null;
            _companionsLostHandle?.Dispose(); _companionsLostHandle = null;
            _companionsFinalHandle?.Dispose(); _companionsFinalHandle = null;
            _cartsHandle?.Dispose(); _cartsHandle = null;
            _repairsHandle?.Dispose(); _repairsHandle = null;
            _animalsLostHandle?.Dispose(); _animalsLostHandle = null;
            _peakLoadHandle?.Dispose(); _peakLoadHandle = null;
        }

        public override void Apply(LifetimeStatsViewState state)
        {
            var data = state.Raw;
            SetArg(_companionsHiredHandle, data.TotalCompanionsHired);
            SetArg(_companionsLostHandle, data.TotalCompanionsLost);
            SetArg(_companionsFinalHandle, data.TotalCompanionsFinal);
            SetArg(_cartsHandle, data.TotalCartsBought);
            SetArg(_repairsHandle, data.TotalRepairs);
            SetArg(_animalsLostHandle, data.TotalAnimalsLost);
            SetArg(_peakLoadHandle, $"{data.PeakCaravanLoad}%");
        }
    }
}
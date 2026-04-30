using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.LifetimeStats.Sections
{
    public class PathLifeStatsSection : LifeStatsSection
    {
        [SerializeField] private TextMeshProUGUI _runsTotalText;
        [SerializeField] private TextMeshProUGUI _winsText;
        [SerializeField] private TextMeshProUGUI _lossesText;
        [SerializeField] private TextMeshProUGUI _daysText;
        [SerializeField] private TextMeshProUGUI _distanceText;
        [SerializeField] private TextMeshProUGUI _citiesText;
        [SerializeField] private TextMeshProUGUI _longestRunText;

        private LocalizationService.LocalizedTextHandle _runsTotalHandle;
        private LocalizationService.LocalizedTextHandle _winsHandle;
        private LocalizationService.LocalizedTextHandle _lossesHandle;
        private LocalizationService.LocalizedTextHandle _daysHandle;
        private LocalizationService.LocalizedTextHandle _distanceHandle;
        private LocalizationService.LocalizedTextHandle _citiesHandle;
        private LocalizationService.LocalizedTextHandle _longestRunHandle;
        
        public override void BindLocalization(LocalizationService localization)
        {
            base.BindLocalization(localization);
            HeaderHandle = BindValue(_header, LocUI.UI_LegacyShop_Lifetime_Section_Adventures, localization);
            
            _runsTotalHandle = BindValue(_runsTotalText, LocUI.UI_LegacyShop_Lifetime_RunsTotal, localization);
            _winsHandle = BindValue(_winsText, LocUI.UI_LegacyShop_Lifetime_Wins, localization);
            _lossesHandle = BindValue(_lossesText, LocUI.UI_LegacyShop_Lifetime_Losses, localization);
            _daysHandle = BindValue(_daysText, LocUI.UI_LegacyShop_Lifetime_Days, localization);
            _distanceHandle = BindValue(_distanceText, LocUI.UI_LegacyShop_Lifetime_Distance, localization);
            _citiesHandle = BindValue(_citiesText, LocUI.UI_LegacyShop_Lifetime_Cities, localization);
            _longestRunHandle = BindValue(_longestRunText, LocUI.UI_LegacyShop_Lifetime_LongestRun, localization);
        }

        public override void DisposeBinding()
        {
            base.DisposeBinding();
            _runsTotalHandle?.Dispose(); _runsTotalHandle = null;
            _winsHandle?.Dispose(); _winsHandle = null;
            _lossesHandle?.Dispose(); _lossesHandle = null;
            _daysHandle?.Dispose(); _daysHandle = null;
            _distanceHandle?.Dispose(); _distanceHandle = null;
            _citiesHandle?.Dispose(); _citiesHandle = null;
            _longestRunHandle?.Dispose(); _longestRunHandle = null;
        }

        public override void Apply(LifetimeStatsViewState state)
        {
            var data = state.Raw;
            SetArg(_runsTotalHandle, data.RunsCompleted);
            SetArg(_winsHandle, data.RunsVictory);
            SetArg(_lossesHandle, data.RunsDefeat);
            SetArg(_daysHandle, data.TotalDaysTravelled);
            SetArg(_distanceHandle, data.TotalDistanceKm);
            SetArg(_citiesHandle, data.TotalCitiesVisited);
            SetArg(_longestRunHandle, data.LongestRunDays);
        }
    }
}
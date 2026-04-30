using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.LifetimeStats.Sections
{
    public class AdventuresLifeStatsSection : LifeStatsSection
    {
        [SerializeField] private TextMeshProUGUI _eventsText;
        [SerializeField] private TextMeshProUGUI _eventsSuccessText;
        [SerializeField] private TextMeshProUGUI _eventsFailText;
        [SerializeField] private TextMeshProUGUI _crisesText;
        [SerializeField] private TextMeshProUGUI _hazardsText;
        [SerializeField] private TextMeshProUGUI _questsCompletedText;
        [SerializeField] private TextMeshProUGUI _questsFailedText;
        
        private LocalizationService.LocalizedTextHandle _eventsHandle;
        private LocalizationService.LocalizedTextHandle _eventsSuccessHandle;
        private LocalizationService.LocalizedTextHandle _eventsFailHandle;
        private LocalizationService.LocalizedTextHandle _crisesHandle;
        private LocalizationService.LocalizedTextHandle _hazardsHandle;
        private LocalizationService.LocalizedTextHandle _questsCompletedHandle;
        private LocalizationService.LocalizedTextHandle _questsFailedHandle;
        
        public override void BindLocalization(LocalizationService localization)
        {
            base.BindLocalization(localization);
            HeaderHandle = BindValue(_header, LocUI.UI_LegacyShop_Lifetime_Section_Adventures, localization);
            _eventsHandle = BindValue(_eventsText, LocUI.UI_LegacyShop_Lifetime_Events, localization);
            _eventsSuccessHandle = BindValue(_eventsSuccessText, LocUI.UI_LegacyShop_Lifetime_EventsSuccess, localization);
            _eventsFailHandle = BindValue(_eventsFailText, LocUI.UI_LegacyShop_Lifetime_EventsFail, localization);
            _crisesHandle = BindValue(_crisesText, LocUI.UI_LegacyShop_Lifetime_Crises, localization);
            _hazardsHandle = BindValue(_hazardsText, LocUI.UI_LegacyShop_Lifetime_Hazards, localization);
            _questsCompletedHandle = BindValue(_questsCompletedText, LocUI.UI_LegacyShop_Lifetime_QuestsCompleted, localization);
            _questsFailedHandle = BindValue(_questsFailedText, LocUI.UI_LegacyShop_Lifetime_QuestsFailed, localization);
        }

        public override void DisposeBinding()
        {
            base.DisposeBinding();
            _eventsHandle?.Dispose(); _eventsHandle = null;
            _eventsSuccessHandle?.Dispose(); _eventsSuccessHandle = null;
            _eventsFailHandle?.Dispose(); _eventsFailHandle = null;
            _crisesHandle?.Dispose(); _crisesHandle = null;
            _hazardsHandle?.Dispose(); _hazardsHandle = null;
            _questsCompletedHandle?.Dispose(); _questsCompletedHandle = null;
            _questsFailedHandle?.Dispose(); _questsFailedHandle = null;
        }

        public override void Apply(LifetimeStatsViewState state)
        {
            var data = state.Raw;
            int totalEvents = data.TotalEventsSuccess + data.TotalEventsFail;
            SetArg(_eventsHandle, totalEvents);
            SetArg(_eventsSuccessHandle, data.TotalEventsSuccess);
            SetArg(_eventsFailHandle, data.TotalEventsFail);
            SetArg(_crisesHandle, data.TotalCrisesSurvived);
            SetArg(_hazardsHandle, data.TotalHazardsFaced);
            SetArg(_questsCompletedHandle, data.TotalQuestsCompleted);
            SetArg(_questsFailedHandle, data.TotalQuestsFailed);
        }
    }
}
using System.Collections.Generic;
using Internal.Scripts.Meta;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.RunEnd.Stats
{
    public sealed class AdventuresStatsView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _headerText;

        [SerializeField] private TextMeshProUGUI _eventsText;
        [SerializeField] private TextMeshProUGUI _eventsSuccessText;
        [SerializeField] private TextMeshProUGUI _eventsFailText;
        [SerializeField] private TextMeshProUGUI _crisesText;
        [SerializeField] private TextMeshProUGUI _hazardsText;
        [SerializeField] private TextMeshProUGUI _questsText;
        [SerializeField] private TextMeshProUGUI _languagesText;

        private LocalizationService.LocalizedTextHandle _headerHandle;

        public void BindLocalization(LocalizationService localization)
        {
            _headerHandle?.Dispose();
            _headerHandle = localization.BindText(_headerText, "UI", LocUI.UI_RunEnd_Section_Adventures, $"{name}.Header");
        }

        public void DisposeBinding()
        {
            _headerHandle?.Dispose();
            _headerHandle = null;
        }

        public void Apply(RunStatsData stats, IReadOnlyList<string> languagesNames)
        {
            _eventsText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_EventsTotal, null, stats.EventsResolvedSuccess + stats.EventsResolvedFail);
            _eventsSuccessText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_EventsSuccess, null, stats.EventsResolvedSuccess);
            _eventsFailText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_EventsFail, null, stats.EventsResolvedFail);
            _crisesText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_CrisesSurvived, null, stats.CrisesSurvived);
            _hazardsText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_TotalHazards, null, stats.TotalHazards);
            
            int total = stats.QuestsCompleted + stats.QuestsFailed;
            _questsText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_QuestsCompleted, null, stats.QuestsCompleted, total);
            
            int count = languagesNames?.Count ?? 0;
            string list = count > 0 ? string.Join(", ", languagesNames) : "";
            _languagesText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_LanguagesLearned, null, count, list);
        }
    }
}

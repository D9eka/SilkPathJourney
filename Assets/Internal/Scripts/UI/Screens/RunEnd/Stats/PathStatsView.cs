using Internal.Scripts.Meta;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.RunEnd.Stats
{
    public sealed class PathStatsView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _headerText;

        [SerializeField] private TextMeshProUGUI _daysText;
        [SerializeField] private TextMeshProUGUI _citiesText;
        [SerializeField] private TextMeshProUGUI _kilometersText;

        private LocalizationService.LocalizedTextHandle _headerHandle;

        public void BindLocalization(LocalizationService localization)
        {
            _headerHandle?.Dispose();
            _headerHandle = localization.BindText(_headerText, "UI", LocUI.UI_RunEnd_Section_Path, $"{name}.Header");
        }

        public void DisposeBinding()
        {
            _headerHandle?.Dispose();
            _headerHandle = null;
        }

        public void Apply(RunStatsData stats)
        {
            _daysText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_DaysTravelled, null, stats.DaysTravelled); 
            _citiesText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_CitiesVisited, null, stats.CitiesVisitedList.Count); 
            _kilometersText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_KilometersTravelled, null, (int)stats.KilometersTravelled);
        }
    }
}

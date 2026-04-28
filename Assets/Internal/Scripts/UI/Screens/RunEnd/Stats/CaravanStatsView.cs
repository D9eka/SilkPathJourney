using Internal.Scripts.Meta;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.RunEnd.Stats
{
    public sealed class CaravanStatsView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _headerText;

        [SerializeField] private TextMeshProUGUI _companionsHiredText;
        [SerializeField] private TextMeshProUGUI _companionsLostText;
        [SerializeField] private TextMeshProUGUI _companionsFinalText;
        [SerializeField] private TextMeshProUGUI _cartsBoughtText;
        [SerializeField] private TextMeshProUGUI _animalsLostText;
        [SerializeField] private TextMeshProUGUI _repairsText;
        [SerializeField] private TextMeshProUGUI _peakLoadText;

        private LocalizationService.LocalizedTextHandle _headerHandle;

        public void BindLocalization(LocalizationService localization)
        {
            _headerHandle?.Dispose();
            _headerHandle = localization.BindText(_headerText, "UI", LocUI.UI_RunEnd_Section_Caravan, $"{name}.Header");
        }

        public void DisposeBinding()
        {
            _headerHandle?.Dispose();
            _headerHandle = null;
        }

        public void Apply(RunStatsData stats)
        { 
            _companionsHiredText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_CompanionsHired, null, stats.CompanionsHired);
            _companionsLostText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_CompanionsLost, null, stats.CompanionsLost);
            _companionsFinalText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_CompanionsFinal, null, stats.CompanionsFinal);
            _cartsBoughtText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_CartsBought, null, stats.CartsBought);
            _animalsLostText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_AnimalsLost, null, stats.AnimalsLost);
            _repairsText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_RepairsDone, null, stats.RepairsDone);
            _peakLoadText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_PeakLoad, null, stats.PeakLoadPercent);
        }
    }
}

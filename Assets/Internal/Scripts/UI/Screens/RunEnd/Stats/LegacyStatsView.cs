using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.RunEnd.Stats
{
    public sealed class LegacyStatsView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _earnedText;
        [SerializeField] private GameObject _bonusGroup;
        [SerializeField] private TextMeshProUGUI _bonusText;

        public void Apply(int earned, int bonus)
        {
            _earnedText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_LegacyEarned, null, earned);
            _bonusGroup.SetActive(bonus > 0);

            if (bonus > 0 && _bonusText != null)
                _bonusText.text = LocalizationService.Resolve("UI", LocUI.UI_RunEnd_Stat_LegacyBonus, null, bonus);
        }
    }
}

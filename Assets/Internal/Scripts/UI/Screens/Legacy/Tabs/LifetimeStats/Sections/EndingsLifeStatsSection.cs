using System.Collections.Generic;
using System.Text;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.LifetimeStats.Sections
{
    public class EndingsLifeStatsSection : LifeStatsSection
    {
        [SerializeField] private TextMeshProUGUI _runsTotalText;
        [SerializeField] private TextMeshProUGUI _bestRunText;
        [SerializeField] private TextMeshProUGUI _deathReasonsText;

        private LocalizationService.LocalizedTextHandle _runsTotalHandle;
        private LocalizationService.LocalizedTextHandle _bestRunHandle;

        public override void BindLocalization(LocalizationService localization)
        {
            base.BindLocalization(localization);
            HeaderHandle = BindValue(_header, LocUI.UI_LegacyShop_Lifetime_Section_Endings, localization);
            _runsTotalHandle = BindValue(_runsTotalText, LocUI.UI_LegacyShop_Lifetime_RunsTotal, localization);
            _bestRunHandle = _bestRunText != null && localization != null
                ? localization.BindText(
                    _bestRunText,
                    new LocalizedString("UI", LocUI.UI_LegacyShop_Lifetime_BestRun),
                    $"{_bestRunText.name}.{LocUI.UI_LegacyShop_Lifetime_BestRun}",
                    "-", null,
                    "-", "-")
                : null;
        }

        public override void DisposeBinding()
        {
            base.DisposeBinding();
            _runsTotalHandle?.Dispose(); _runsTotalHandle = null;
            _bestRunHandle?.Dispose(); _bestRunHandle = null;
        }

        public override void Apply(LifetimeStatsViewState state)
        {
            var data = state.Raw;
            SetArg(_runsTotalHandle, data.RunsCompleted);
            SetArg(_bestRunHandle, data.BestRunDays, data.BestRunLegacyEarned);
            ApplyDeathReasons(data);
        }

        private void ApplyDeathReasons(Internal.Scripts.Meta.LifetimeStatsData data)
        {
            if (_deathReasonsText == null) return;

            var sorted = new List<(string key, int count)>
            {
                (LocUI.UI_LegacyShop_Lifetime_End_Bankruptcy, data.EndCount_Bankruptcy),
                (LocUI.UI_LegacyShop_Lifetime_End_Mutiny, data.EndCount_Mutiny),
                (LocUI.UI_LegacyShop_Lifetime_End_CaravanLost, data.EndCount_CaravanLost),
                (LocUI.UI_LegacyShop_Lifetime_End_Hunger, data.EndCount_Famine),
            };
            sorted.Sort((a, b) => b.count.CompareTo(a.count));

            var sb = new StringBuilder();
            sb.Append(LocalizationService.Resolve("UI", LocUI.UI_LegacyShop_Lifetime_DeathReasons,
                LocUI.UI_LegacyShop_Lifetime_DeathReasons));

            for (int i = 0; i < 3 && i < sorted.Count; i++)
            {
                if (sorted[i].count <= 0) break;
                sb.Append('\n');
                sb.Append(LocalizationService.Resolve("UI", sorted[i].key, sorted[i].key, sorted[i].count));
            }

            _deathReasonsText.text = sb.ToString();
        }
    }
}

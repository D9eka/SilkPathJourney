using System.Text;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using UnityEngine.Localization;

namespace Internal.Scripts.Trading
{
    public static class PriceTooltipFormatter
    {
        private const float EPSILON = 0.005f;

        public static (string title, string description) Format(PriceBreakdown b)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{LocalizationService.Resolve(LocUI.Table, LocUI.UI_Tooltip_BasePrice)}: {b.BasePrice}");

            if (!IsApproximatelyOne(b.MarketMult))
            {
                string label = b.IsNpcTrade
                    ? LocalizationService.Resolve(LocUI.Table, LocUI.UI_Tooltip_NpcMarkup)
                    : LocalizationService.Resolve(LocUI.Table, LocUI.UI_Tooltip_Market);
                sb.AppendLine($"{label}: x{b.MarketMult:0.##}");
            }

            if (!b.IsNpcTrade && !IsApproximatelyOne(b.BonusMult))
                sb.AppendLine($"{LocalizationService.Resolve(LocUI.Table, LocUI.UI_Tooltip_Bonus)}: x{b.BonusMult:0.##}");

            if (!b.IsNpcTrade && !IsApproximatelyOne(b.ModifierMult))
                sb.AppendLine($"{LocalizationService.Resolve(LocUI.Table, LocUI.UI_Tooltip_Modifiers)}: x{b.ModifierMult:0.##}");

            if (!IsApproximatelyOne(b.TitheMult))
                sb.AppendLine($"{LocalizationService.ResolveString(new LocalizedString(LocUI.Table, "UI.Tooltip.GuildTithe"), "UI.Tooltip.GuildTithe", "UI.Tooltip.GuildTithe")}: x{b.TitheMult:0.##}");

            sb.Append($"{LocalizationService.Resolve(LocUI.Table, LocUI.UI_Tooltip_Total)}: {b.FinalPrice}");

            return (b.ItemName, sb.ToString());
        }

        private static bool IsApproximatelyOne(float value)
            => value >= 1f - EPSILON && value <= 1f + EPSILON;
    }
}

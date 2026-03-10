using System.Text;
using Internal.Scripts.UI.Localization;
using UnityEngine.Localization;

namespace Internal.Scripts.Trading
{
    public static class PriceTooltipFormatter
    {
        private const float EPSILON = 0.005f;
        private const string TABLE = "UI";

        public static (string title, string description) Format(PriceBreakdown b)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Loc("UI.Tooltip.BasePrice", "Базовая цена")}: {b.BasePrice}");

            if (!IsApproximatelyOne(b.MarketMult))
            {
                string label = b.IsNpcTrade
                    ? Loc("UI.Tooltip.NpcMarkup", "Наценка")
                    : Loc("UI.Tooltip.Market", "Рынок");
                sb.AppendLine($"{label}: x{b.MarketMult:0.##}");
            }

            if (!b.IsNpcTrade && !IsApproximatelyOne(b.BonusMult))
                sb.AppendLine($"{Loc("UI.Tooltip.Bonus", "Бонусы")}: x{b.BonusMult:0.##}");

            sb.Append($"{Loc("UI.Tooltip.Total", "Итого")}: {b.FinalPrice}");

            return (b.ItemName, sb.ToString());
        }

        private static string Loc(string key, string fallback)
            => LocalizationService.ResolveString(new LocalizedString(TABLE, key), fallback, key);

        private static bool IsApproximatelyOne(float value)
            => value >= 1f - EPSILON && value <= 1f + EPSILON;
    }
}

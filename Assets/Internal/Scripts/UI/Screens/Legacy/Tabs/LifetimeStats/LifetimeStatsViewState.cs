using Internal.Scripts.Meta;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.LifetimeStats
{
    public sealed class LifetimeStatsViewState
    {
        public readonly LifetimeStatsData Raw;
        public readonly string BestDealText;
        public readonly string FavoriteItemText;
        public readonly string MostExpensiveItemText;
        public readonly string TopLanguageText;

        public LifetimeStatsViewState(LifetimeStatsData raw,
            string bestDealText, string favoriteItemText, string mostExpensiveItemText, string topLanguageText)
        {
            Raw = raw;
            BestDealText = bestDealText;
            FavoriteItemText = favoriteItemText;
            MostExpensiveItemText = mostExpensiveItemText;
            TopLanguageText = topLanguageText;
        }
    }
}

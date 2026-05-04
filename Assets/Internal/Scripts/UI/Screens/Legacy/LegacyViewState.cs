namespace Internal.Scripts.UI.Screens.Legacy
{
    public sealed class LegacyViewState
    {
        public readonly int LegacyPointsTotal;
        public readonly LegacyTab ActiveTab;
        public readonly object ActiveTabState;

        public LegacyViewState(
            int legacyPointsTotal,
            LegacyTab activeTab,
            object activeTabState)
        {
            LegacyPointsTotal = legacyPointsTotal;
            ActiveTab = activeTab;
            ActiveTabState = activeTabState;
        }
    }
}

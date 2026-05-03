using System.Collections.Generic;

namespace Internal.Scripts.UI.Screens.Barracks
{
    public sealed class BarracksViewState
    {
        public readonly IReadOnlyList<BarracksEntry> ScrollEntries;
        public readonly EliteGuardEntry EliteGuard;
        public readonly int PlayerMoney;

        public BarracksViewState(IReadOnlyList<BarracksEntry> scrollEntries, EliteGuardEntry eliteGuard, int playerMoney)
        {
            ScrollEntries = scrollEntries;
            EliteGuard = eliteGuard;
            PlayerMoney = playerMoney;
        }
    }
}

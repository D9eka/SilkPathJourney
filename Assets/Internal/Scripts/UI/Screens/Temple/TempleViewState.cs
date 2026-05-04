using System.Collections.Generic;

namespace Internal.Scripts.UI.Screens.Temple
{
    public sealed class TempleViewState
    {
        public readonly IReadOnlyList<TempleEntry> Entries;
        public readonly int PlayerMoney;
        public readonly int PlayerMorale;
        public readonly int PlayerDanger;

        public TempleViewState(IReadOnlyList<TempleEntry> entries, int playerMoney, int playerMorale, int playerDanger)
        {
            Entries = entries;
            PlayerMoney = playerMoney;
            PlayerMorale = playerMorale;
            PlayerDanger = playerDanger;
        }
    }
}

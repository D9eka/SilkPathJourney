using System.Collections.Generic;
using Internal.Scripts.Meta;

namespace Internal.Scripts.UI.Screens.RunEnd
{
    public sealed class RunEndViewState
    {
        public readonly EndType EndType;
        public readonly string ReasonKey;
        public readonly RunStatsData Stats;
        public readonly int LegacyPointsEarned;
        public readonly int LegacyPointsBonus;
        public readonly int LegacyPointsTotal;
        public readonly string BestDealName;
        public readonly IReadOnlyList<string> LanguagesNames;

        public RunEndViewState(EndType endType, string reasonKey, RunStatsData stats,
            int legacyPointsEarned, int legacyPointsBonus, int legacyPointsTotal,
            string bestDealName, IReadOnlyList<string> languagesNames)
        {
            EndType = endType;
            ReasonKey = reasonKey;
            Stats = stats;
            LegacyPointsEarned = legacyPointsEarned;
            LegacyPointsBonus = legacyPointsBonus;
            LegacyPointsTotal = legacyPointsTotal;
            BestDealName = bestDealName;
            LanguagesNames = languagesNames;
        }
    }
}

using UnityEngine;

namespace Internal.Scripts.Meta
{
    public sealed class LegacyPointsCalculator
    {
        public int Calculate(RunStatsData stats, EndType endType, out int defeatBonus)
        {
            if (stats == null) { defeatBonus = 0; return 0; }

            int raw = stats.DaysTravelled
                    + Mathf.Max(0, stats.CitiesVisitedList.Count - 1) * 5
                    + Mathf.Max(0, (stats.MoneyEarned - stats.MoneySpent) / 10)
                    + stats.QuestsCompleted * 10
                    + stats.LanguageLevelsGained * 5
                    + stats.CrisesSurvived * 10;

            bool isDefeat = endType != EndType.None && endType != EndType.Victory;
            defeatBonus = isDefeat ? raw / 2 : 0;
            return raw + defeatBonus;
        }
    }
}

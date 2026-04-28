using System;
using System.Collections.Generic;

namespace Internal.Scripts.Meta
{
    [Serializable]
    public sealed class PersistentProgressData
    {
        public int LegacyPoints;
        public List<string> UnlockedIds = new();
        public List<string> EarnedAchievementIds = new();
        public LifetimeStatsData Lifetime = new();
    }

    [Serializable]
    public sealed class LifetimeStatsData
    {
        public int RunsCompleted;
        public int RunsVictory;
        public int RunsDefeat;
        public int TotalDaysTravelled;
        public int TotalMoneyEarned;
        public int BestRunDays;
        public string BestRunEndType;
        public int BestRunLegacyEarned;
    }
}

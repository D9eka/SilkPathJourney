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
        public List<string> AchievementEarnedDates = new();
        public LifetimeStatsData Lifetime = new();

        public string GetEarnedDate(string achievementId)
        {
            int idx = EarnedAchievementIds.IndexOf(achievementId);
            if (idx < 0 || idx >= AchievementEarnedDates.Count) return null;
            return AchievementEarnedDates[idx];
        }

        public void SetEarnedDate(string achievementId, string date)
        {
            int idx = EarnedAchievementIds.IndexOf(achievementId);
            if (idx < 0) return;
            while (AchievementEarnedDates.Count <= idx)
                AchievementEarnedDates.Add(string.Empty);
            AchievementEarnedDates[idx] = date;
        }
    }

    [Serializable]
    public sealed class LifetimeStatsData
    {
        // General
        public int RunsCompleted;
        public int RunsVictory;
        public int RunsDefeat;
        public int TotalDaysTravelled;
        public int TotalDistanceKm;
        public int TotalCitiesVisited;
        public int LongestRunDays;

        // Economy
        public int TotalMoneyEarned;
        public int TotalMoneySpent;
        public int TotalItemsBought;
        public int TotalItemsSold;
        public string BestDealItemId;
        public int BestDealProfit;
        public string FavoriteItemId;
        public int FavoriteItemCount;
        public string MostExpensiveItemId;
        public int MostExpensivePrice;
        public int TotalLegacyEarned;

        // Adventures
        public int TotalEventsSuccess;
        public int TotalEventsFail;
        public int TotalCrisesSurvived;
        public int TotalHazardsFaced;
        public int TotalQuestsCompleted;
        public int TotalQuestsFailed;
        public int TotalLanguagesLearned;

        // Caravan
        public int TotalCompanionsHired;
        public int TotalCompanionsLost;
        public int TotalCompanionsFinal;
        public int TotalCartsBought;
        public int TotalRepairs;
        public int TotalAnimalsLost;
        public int PeakCaravanLoad;

        // Social
        public int FriendlyCitiesCount;
        public int HostileCitiesCount;
        public int CaravansHelpedCount;
        public int ConflictsResolvedCount;
        public int UniqueNpcMetCount;
        public string TopLanguageId;

        // Endings
        public int BestRunDays;
        public string BestRunEndType;
        public int BestRunLegacyEarned;
        public int EndCount_Victory;
        public int EndCount_Bankruptcy;
        public int EndCount_Mutiny;
        public int EndCount_CaravanLost;
        public int EndCount_Famine;
    }
}

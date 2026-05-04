using System;
using System.Collections.Generic;
using Internal.Scripts.Save;

namespace Internal.Scripts.Meta
{
    public sealed class PersistentProgressService
    {
        private const string FILE_NAME = "persistent_progress.json";

        private readonly IJsonStorage _storage;
        private PersistentProgressData _data;

        public int LegacyPoints => _data.LegacyPoints;
        public IReadOnlyList<string> UnlockedIds => _data.UnlockedIds;
        public IReadOnlyList<string> EarnedAchievementIds => _data.EarnedAchievementIds;
        public LifetimeStatsData Lifetime => _data.Lifetime;

        public PersistentProgressService(IJsonStorage storage)
        {
            _storage = storage;
            Load();
        }

        private void Load()
        {
            _data = _storage.Load<PersistentProgressData>(FILE_NAME) ?? new PersistentProgressData();
            MigrateMissingEarnedDates();
        }

        private void MigrateMissingEarnedDates()
        {
            if (_data.EarnedAchievementIds.Count <= _data.AchievementEarnedDates.Count) return;

            string today = DateTime.Now.ToString("yyyy-MM-dd");
            while (_data.AchievementEarnedDates.Count < _data.EarnedAchievementIds.Count)
                _data.AchievementEarnedDates.Add(today);

            Save();
        }

        public void Save()
        {
            _storage.Save(FILE_NAME, _data);
        }

        public void AddLegacyPoints(int amount)
        {
            if (amount <= 0) return;
            _data.LegacyPoints += amount;
            Save();
        }

        public bool TrySpendLegacyPoints(int amount)
        {
            if (amount <= 0 || _data.LegacyPoints < amount)
                return false;
            _data.LegacyPoints -= amount;
            Save();
            return true;
        }

        public bool Unlock(string id)
        {
            if (string.IsNullOrEmpty(id) || _data.UnlockedIds.Contains(id))
                return false;
            _data.UnlockedIds.Add(id);
            Save();
            return true;
        }

        public bool RecordAchievement(string id)
        {
            if (string.IsNullOrEmpty(id) || _data.EarnedAchievementIds.Contains(id))
                return false;
            _data.EarnedAchievementIds.Add(id);
            _data.SetEarnedDate(id, DateTime.Now.ToString("yyyy-MM-dd"));
            Save();
            return true;
        }

        public string GetAchievementEarnedDate(string id) => _data.GetEarnedDate(id);

        public void RecordRunCompleted(RunStatsData stats, EndType endType, int legacyEarned)
        {
            var lt = _data.Lifetime;
            lt.RunsCompleted++;

            switch (endType)
            {
                case EndType.Victory:
                    lt.RunsVictory++;
                    lt.EndCount_Victory++;
                    break;
                case EndType.Defeat_Bankruptcy:
                    lt.RunsDefeat++;
                    lt.EndCount_Bankruptcy++;
                    break;
                case EndType.Defeat_Mutiny:
                    lt.RunsDefeat++;
                    lt.EndCount_Mutiny++;
                    break;
                case EndType.Defeat_CaravanLost:
                    lt.RunsDefeat++;
                    lt.EndCount_CaravanLost++;
                    break;
                case EndType.Defeat_Famine:
                    lt.RunsDefeat++;
                    lt.EndCount_Famine++;
                    break;
            }

            if (stats != null)
            {
                lt.TotalDaysTravelled += stats.DaysTravelled;
                lt.TotalDistanceKm += (int)stats.KilometersTravelled;
                lt.TotalCitiesVisited += stats.CitiesVisitedList.Count;
                lt.TotalMoneyEarned += stats.MoneyEarned;
                lt.TotalMoneySpent += stats.MoneySpent;
                lt.TotalItemsBought += stats.ItemsBought;
                lt.TotalItemsSold += stats.ItemsSold;
                lt.TotalEventsSuccess += stats.EventsResolvedSuccess;
                lt.TotalEventsFail += stats.EventsResolvedFail;
                lt.TotalCrisesSurvived += stats.CrisesSurvived;
                lt.TotalHazardsFaced += stats.TotalHazards;
                lt.TotalQuestsCompleted += stats.QuestsCompleted;
                lt.TotalQuestsFailed += stats.QuestsFailed;
                lt.TotalLanguagesLearned += stats.LanguagesLearnedList.Count;
                lt.TotalCompanionsHired += stats.CompanionsHired;
                lt.TotalCompanionsLost += stats.CompanionsLost;
                lt.TotalCompanionsFinal += stats.CompanionsFinal;
                lt.TotalCartsBought += stats.CartsBought;
                lt.TotalRepairs += stats.RepairsDone;
                lt.TotalAnimalsLost += stats.AnimalsLost;
                lt.TotalLegacyEarned += legacyEarned;

                if (stats.PeakLoadPercent > lt.PeakCaravanLoad)
                    lt.PeakCaravanLoad = stats.PeakLoadPercent;

                if (stats.DaysTravelled > lt.LongestRunDays)
                    lt.LongestRunDays = stats.DaysTravelled;

                if (stats.DaysTravelled > lt.BestRunDays)
                {
                    lt.BestRunDays = stats.DaysTravelled;
                    lt.BestRunEndType = stats.EndReasonKey ?? endType.ToString();
                    lt.BestRunLegacyEarned = legacyEarned;
                }

                if (stats.BestDeal != null && stats.BestDeal.Profit > 0 && !string.IsNullOrEmpty(stats.BestDeal.ItemId))
                {
                    if (stats.BestDeal.Profit > lt.BestDealProfit)
                    {
                        lt.BestDealItemId = stats.BestDeal.ItemId;
                        lt.BestDealProfit = stats.BestDeal.Profit;
                    }
                }
            }

            Save();
        }
    }
}

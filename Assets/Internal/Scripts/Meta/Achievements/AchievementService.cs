using System.Linq;

namespace Internal.Scripts.Meta.Achievements
{
    public sealed class AchievementService
    {
        private readonly AchievementDatabase _database;
        private readonly PersistentProgressService _persistent;

        public AchievementService(AchievementDatabase database, PersistentProgressService persistent)
        {
            _database = database;
            _persistent = persistent;
        }

        public void CheckAll(RunStatsData stats, EndType endType)
        {
            if (_database == null || stats == null) return;

            foreach (var achievement in _database.All)
            {
                if (achievement == null || string.IsNullOrEmpty(achievement.Id)) continue;
                if (_persistent.EarnedAchievementIds.Contains(achievement.Id)) continue;

                if (IsConditionMet(achievement, stats, endType))
                {
                    _persistent.RecordAchievement(achievement.Id);
                    if (achievement.LegacyReward > 0)
                        _persistent.AddLegacyPoints(achievement.LegacyReward);
                }
            }
        }

        private static bool IsConditionMet(AchievementData achievement, RunStatsData stats, EndType endType)
        {
            return achievement.Trigger switch
            {
                AchievementTrigger.DaysTravelled => stats.DaysTravelled >= achievement.Value,
                AchievementTrigger.CitiesVisited => stats.CitiesVisitedList.Count >= achievement.Value,
                AchievementTrigger.MoneyEarned => stats.MoneyEarned >= achievement.Value,
                AchievementTrigger.QuestsCompleted => stats.QuestsCompleted >= achievement.Value,
                AchievementTrigger.Victory => endType == EndType.Victory,
                AchievementTrigger.DefeatSurvived => endType != EndType.None && endType != EndType.Victory,
                AchievementTrigger.LanguagesLearned => stats.LanguagesLearnedList.Count >= achievement.Value,
                _ => false
            };
        }
    }
}

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

        public void CheckAll(EndType endType)
        {
            if (_database == null) return;

            foreach (var achievement in _database.All)
            {
                if (achievement == null || string.IsNullOrEmpty(achievement.Id)) continue;
                if (_persistent.EarnedAchievementIds.Contains(achievement.Id)) continue;

                if (IsConditionMet(achievement, endType))
                {
                    _persistent.RecordAchievement(achievement.Id);
                    if (achievement.LegacyReward > 0)
                        _persistent.AddLegacyPoints(achievement.LegacyReward);
                }
            }
        }

        public void CheckLifetime() => CheckAll(EndType.None);

        private bool IsConditionMet(AchievementData achievement, EndType endType)
        {
            var lt = _persistent.Lifetime;
            return achievement.Trigger switch
            {
                AchievementTrigger.DaysTravelled => lt.TotalDaysTravelled >= achievement.Value,
                AchievementTrigger.CitiesVisited => lt.TotalCitiesVisited >= achievement.Value,
                AchievementTrigger.MoneyEarned => lt.TotalMoneyEarned >= achievement.Value,
                AchievementTrigger.QuestsCompleted => lt.TotalQuestsCompleted >= achievement.Value,
                AchievementTrigger.Victory => endType == EndType.Victory || lt.RunsVictory > 0,
                AchievementTrigger.DefeatSurvived => (endType != EndType.None && endType != EndType.Victory) || lt.RunsDefeat > 0,
                AchievementTrigger.LanguagesLearned => lt.TotalLanguagesLearned >= achievement.Value,
                _ => false
            };
        }
    }
}

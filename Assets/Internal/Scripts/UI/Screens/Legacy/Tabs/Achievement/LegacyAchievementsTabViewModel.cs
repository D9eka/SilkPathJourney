using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Internal.Scripts.Meta;
using Internal.Scripts.Meta.Achievements;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Theme;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.Achievement
{
    public sealed class LegacyAchievementsTabViewModel
    {
        private readonly PersistentProgressService _persistent;
        private readonly AchievementDatabase _achievementDatabase;
        
        public readonly UiThemeService ThemeService;

        public LegacyAchievementsTabViewModel(PersistentProgressService persistent, 
            AchievementDatabase achievementDatabase, UiThemeService themeService)
        {
            _persistent = persistent;
            _achievementDatabase = achievementDatabase;
            ThemeService = themeService;
        }

        public LegacyAchievementsTabState BuildState()
        {
            return new LegacyAchievementsTabState(BuildAchievementEntries());
        }

        private List<AchievementEntryData> BuildAchievementEntries()
        {
            var result = new List<AchievementEntryData>();
            if (_achievementDatabase == null) return result;

            foreach (var ach in _achievementDatabase.All)
            {
                if (ach == null) continue;
                string achName = ach.Name != null
                    ? LocalizationService.ResolveString(ach.Name, ach.Id, ach.Id)
                    : ach.Id;
                string desc = ach.Description != null
                    ? LocalizationService.ResolveString(ach.Description, string.Empty, ach.Id)
                    : string.Empty;
                bool isEarned = _persistent.EarnedAchievementIds.Contains(ach.Id, StringComparer.Ordinal);
                int progress = ResolveProgress(ach);
                string earnedDate = isEarned ? FormatEarnedDate(_persistent.GetAchievementEarnedDate(ach.Id)) : null;
                result.Add(new AchievementEntryData(ach.Id, achName, desc, ach.LegacyReward, isEarned, ach.Value, progress, earnedDate, ach.Icon));
            }
            return result;
        }

        private int ResolveProgress(AchievementData ach)
        {
            var lt = _persistent.Lifetime;
            return ach.Trigger switch
            {
                AchievementTrigger.DaysTravelled => lt.TotalDaysTravelled,
                AchievementTrigger.MoneyEarned => lt.TotalMoneyEarned,
                AchievementTrigger.Victory => lt.RunsVictory,
                AchievementTrigger.DefeatSurvived => lt.RunsDefeat,
                AchievementTrigger.CitiesVisited => lt.TotalCitiesVisited,
                AchievementTrigger.QuestsCompleted => lt.TotalQuestsCompleted,
                AchievementTrigger.LanguagesLearned => lt.TotalLanguagesLearned,
                _ => 0
            };
        }

        private static string FormatEarnedDate(string isoDate)
        {
            if (string.IsNullOrEmpty(isoDate)) return null;
            if (DateTime.TryParseExact(isoDate, "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dt))
                return dt.ToString("d MMM", CultureInfo.CurrentCulture);
            return isoDate;
        }
    }
}

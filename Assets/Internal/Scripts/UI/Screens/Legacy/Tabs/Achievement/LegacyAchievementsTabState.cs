using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.Achievement
{
    public sealed class AchievementEntryData
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Description;
        public readonly int LegacyReward;
        public readonly bool IsEarned;
        public readonly int TargetValue;
        public readonly int CurrentProgress;
        public readonly string EarnedDate;
        public readonly Sprite Icon;

        public AchievementEntryData(string id, string name, string description, int legacyReward, bool isEarned,
            int targetValue = 0, int currentProgress = 0, string earnedDate = null, Sprite icon = null)
        {
            Id = id;
            Name = name;
            Description = description;
            LegacyReward = legacyReward;
            IsEarned = isEarned;
            TargetValue = targetValue;
            CurrentProgress = currentProgress;
            EarnedDate = earnedDate;
            Icon = icon;
        }
    }

    public sealed class LegacyAchievementsTabState
    {
        public readonly IReadOnlyList<AchievementEntryData> Achievements;

        public LegacyAchievementsTabState(IReadOnlyList<AchievementEntryData> achievements)
        {
            Achievements = achievements;
        }
    }
}

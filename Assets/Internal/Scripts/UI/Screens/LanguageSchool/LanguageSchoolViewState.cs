using System.Collections.Generic;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;

namespace Internal.Scripts.UI.Screens.LanguageSchool
{
    public readonly struct LanguageSchoolEntry
    {
        public readonly LanguageType Language;
        public readonly string Name;
        public readonly LanguageProficiency CurrentLevel;
        public readonly LanguageProficiency NextLevel;
        public readonly int Cost;
        public readonly int Days;
        public readonly bool CanLearn;
        public readonly string CurrentLevelFormatted;
        public readonly string NextLevelFormatted;
        public readonly string DaysFormatted;
        public readonly string LearnButtonText;

        public LanguageSchoolEntry(LanguageType language, string name,
            LanguageProficiency currentLevel, LanguageProficiency nextLevel,
            int cost, int days, bool canLearn,
            string currentLevelFormatted, string nextLevelFormatted,
            string daysFormatted, string learnButtonText)
        {
            Language = language;
            Name = name;
            CurrentLevel = currentLevel;
            NextLevel = nextLevel;
            Cost = cost;
            Days = days;
            CanLearn = canLearn;
            CurrentLevelFormatted = currentLevelFormatted;
            NextLevelFormatted = nextLevelFormatted;
            DaysFormatted = daysFormatted;
            LearnButtonText = learnButtonText;
        }
    }

    public sealed class LanguageSchoolViewState
    {
        public readonly IReadOnlyList<LanguageSchoolEntry> Entries;
        public readonly int PlayerMoney;
        public readonly int PlayerFood;

        public LanguageSchoolViewState(IReadOnlyList<LanguageSchoolEntry> entries, int playerMoney, int playerFood)
        {
            Entries = entries;
            PlayerMoney = playerMoney;
            PlayerFood = playerFood;
        }
    }
}

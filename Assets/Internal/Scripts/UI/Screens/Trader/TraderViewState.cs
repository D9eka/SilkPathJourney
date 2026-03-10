using System.Collections.Generic;

namespace Internal.Scripts.UI.Screens.Trader
{
    public readonly struct TraderViewState
    {
        public readonly IReadOnlyList<ProfileEntry> ProfileItems;
        public readonly IReadOnlyList<SkillViewData> Skills;
        public readonly IReadOnlyList<LanguageViewData> Languages;

        public TraderViewState(IReadOnlyList<ProfileEntry> profileItems, IReadOnlyList<SkillViewData> skills,
            IReadOnlyList<LanguageViewData> languages)
        {
            ProfileItems = profileItems;
            Skills = skills;
            Languages = languages;
        }
    }
}

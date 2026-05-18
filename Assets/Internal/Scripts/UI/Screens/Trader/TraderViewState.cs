using System.Collections.Generic;

namespace Internal.Scripts.UI.Screens.Trader
{
    public readonly struct TraderViewState
    {
        public readonly ProfileEntry Cart;
        public readonly ProfileEntry Backstory;
        public readonly IReadOnlyList<SkillViewData> Skills;
        public readonly IReadOnlyList<LanguageViewData> Languages;

        public TraderViewState(ProfileEntry cart, ProfileEntry backstory,
            IReadOnlyList<SkillViewData> skills, IReadOnlyList<LanguageViewData> languages)
        {
            Cart = cart;
            Backstory = backstory;
            Skills = skills;
            Languages = languages;
        }
    }
}

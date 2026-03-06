using System.Collections.Generic;

namespace Internal.Scripts.UI.Screens.Trader
{
    public readonly struct TraderViewState
    {
        public readonly IReadOnlyList<ProfileEntry> ProfileItems;
        public readonly IReadOnlyList<SkillViewData> Skills;

        public TraderViewState(IReadOnlyList<ProfileEntry> profileItems, IReadOnlyList<SkillViewData> skills)
        {
            ProfileItems = profileItems;
            Skills = skills;
        }
    }
}

using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Trader
{
    public readonly struct SkillViewData
    {
        public readonly LocalizedString Name;
        public readonly LocalizedString Description;
        public readonly float Progress;
        public readonly string Value;

        public SkillViewData(LocalizedString name, LocalizedString description, float progress, string value)
        {
            Name = name;
            Description = description;
            Progress = progress;
            Value = value;
        }
    }
}

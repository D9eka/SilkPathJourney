using Internal.Scripts.Player.Languages;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Trader
{
    public readonly struct LanguageViewData
    {
        public readonly LocalizedString Name;
        public readonly LocalizedString Description;
        public readonly LocalizedString Value;
        public readonly LanguageProficiency Proficiency;
        public readonly float Progress;

        public LanguageViewData(LocalizedString name, LocalizedString description,
            LanguageProficiency proficiency, LocalizedString value)
        {
            Name = name;
            Description = description;
            Value = value;
            Proficiency = proficiency;
            Progress = (float)proficiency / (float)LanguageProficiency.Native;
        }
    }
}

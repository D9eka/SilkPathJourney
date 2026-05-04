using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Components
{
    public class SkillView : MonoBehaviour
    {
        [SerializeField] private LabeledBarView _bar;

        public void Initialize(LocalizationService loc, LocalizedString name, LocalizedString description,
            float progress, string value) =>
            _bar.Initialize(loc, name, description, progress, value);

        public void Initialize(LocalizationService loc, LocalizedString name, LocalizedString description,
            float progress, LocalizedString value) =>
            _bar.Initialize(loc, name, description, progress, value);

        public static LocalizedString ResolveSkillName(SkillType skillType) =>
            skillType switch
            {
                SkillType.Trade => new LocalizedString(LocUI.Table, LocUI.UI_Trader_Skill_Trade_Name),
                SkillType.Charisma => new LocalizedString(LocUI.Table, LocUI.UI_Trader_Skill_Charisma_Name),
                SkillType.Survival => new LocalizedString(LocUI.Table, LocUI.UI_Trader_Skill_Survival_Name),
                _ => null
            };

        public static LocalizedString ResolveLanguageName(LanguageType langType) =>
            new LocalizedString(LocUI.Table, $"UI.Language.{langType}.Name");

        public static LocalizedString ResolveProficiencyName(LanguageProficiency proficiency) =>
            new LocalizedString(LocUI.Table, $"UI.Language.Proficiency.{proficiency}");

        public static float ProgressFor(LanguageProficiency proficiency) =>
            (float)proficiency / (float)LanguageProficiency.Native;
    }
}

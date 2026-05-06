using System;
using System.Collections.Generic;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Player.Skills;
using UnityEngine;
using UnityEngine.Localization;
namespace Internal.Scripts.Camp
{
    [Serializable]
    public struct RepeatSideEffect
    {
        public int RepeatDay;
        public EventOutcomeType Resource;
        public float Value;
        [Range(0f, 1f)] public float EventChance;
    }

    [CreateAssetMenu(menuName = "SPJ/Camp/Camp Action Data", fileName = "CampActionData")]
    public class CampActionData : ScriptableObject
    {
        [field: SerializeField] public CampActionType Type { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public float CostSupplies { get; private set; }
        [field: SerializeField] public EventOutcomeType AffectedResource { get; private set; }

        [Header("Localization")]
        [field: SerializeField] public LocalizedString Name { get; private set; }
        [field: SerializeField] public LocalizedString Hint1 { get; private set; }
        [field: SerializeField] public LocalizedString Hint2 { get; private set; }
        [field: SerializeField] public LocalizedString Hint3Plus { get; private set; }

        [Header("Skill")]
        [field: SerializeField] public SkillType RelatedSkill { get; private set; } = SkillType.None;

        [Header("Repeat")]
        [Tooltip("0 = unlimited. Blocks execution once repeat count reaches this value.")]
        [field: SerializeField] public int MaxRepeatPerSegment { get; private set; }
        [Tooltip("Multiplier per repeat index. Out-of-range repeats clamp to last entry. Empty = no diminishing.")]
        [field: SerializeField] public float[] DiminishingCurve { get; private set; } = Array.Empty<float>();

        [Header("Daily Costs")]
        [Tooltip("Если true — DailyTravelCosts пропустит дневной расход еды в день этого действия.")]
        [field: SerializeField] public bool SkipDailyFoodConsumption { get; private set; }

        [Header("Repeat Side Effects")]
        [field: SerializeField] public List<RepeatSideEffect> SideEffects { get; private set; } = new();

        public string GetName() => Name?.GetLocalizedString();

        public string GetHint(int repeatDays) => repeatDays switch
        {
            <= 0 => null,
            1    => Hint1?.GetLocalizedString(),
            2    => Hint2?.GetLocalizedString(),
            _    => Hint3Plus?.GetLocalizedString(),
        };

#if UNITY_EDITOR
        public void ApplyImport(
            CampActionType type, float costSupplies, EventOutcomeType affectedResource,
            SkillType relatedSkill, int maxRepeat, float[] diminishingCurve,
            bool skipFood, List<RepeatSideEffect> sideEffects,
            LocalizedString name, LocalizedString hint1, LocalizedString hint2, LocalizedString hint3Plus)
        {
            Type = type;
            CostSupplies = costSupplies;
            AffectedResource = affectedResource;
            RelatedSkill = relatedSkill;
            MaxRepeatPerSegment = maxRepeat;
            DiminishingCurve = diminishingCurve ?? Array.Empty<float>();
            SkipDailyFoodConsumption = skipFood;
            SideEffects = sideEffects ?? new List<RepeatSideEffect>();
            Name = name;
            Hint1 = hint1;
            Hint2 = hint2;
            Hint3Plus = hint3Plus;
        }
#endif
    }
}

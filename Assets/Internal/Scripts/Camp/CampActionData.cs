using System;
using System.Collections.Generic;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Player.Skills;
using UnityEngine;

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
        [field: SerializeField] public float CostSupplies { get; private set; }
        [field: SerializeField] public EventOutcomeType AffectedResource { get; private set; }
        [field: SerializeField] public float BaseEffect { get; private set; }

        [Header("Skill")]
        [field: SerializeField] public SkillType RelatedSkill { get; private set; } = SkillType.None;

        [Header("Repeat")]
        [Tooltip("0 = unlimited. Blocks execution once repeat count reaches this value.")]
        [field: SerializeField] public int MaxRepeatPerSegment { get; private set; }
        [Tooltip("Multiplier per repeat index. Out-of-range repeats clamp to last entry. Empty = no diminishing.")]
        [field: SerializeField] public float[] DiminishingCurve { get; private set; } = Array.Empty<float>();

        [Header("Repeat Side Effects")]
        [field: SerializeField] public List<RepeatSideEffect> SideEffects { get; private set; } = new();
    }
}

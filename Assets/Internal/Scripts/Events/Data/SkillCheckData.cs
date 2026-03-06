using System;
using System.Collections.Generic;
using Internal.Scripts.Player.Skills;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Events.Data
{
    [Serializable]
    public struct SkillCheckData
    {
        [field: SerializeField] public int ChoiceIndex { get; private set; }
        [field: SerializeField] public SkillType SkillType { get; private set; }
        [field: SerializeField] public float BaseChance { get; private set; }
        [field: SerializeField] public LocalizedString FailResultText { get; private set; }
        [field: SerializeField] public List<EventOutcomeEntry> FailOutcomes { get; private set; }

#if UNITY_EDITOR
        public SkillCheckData(int choiceIndex, SkillType skillType, float baseChance,
            LocalizedString failResultText, List<EventOutcomeEntry> failOutcomes)
        {
            ChoiceIndex = choiceIndex;
            SkillType = skillType;
            BaseChance = baseChance;
            FailResultText = failResultText;
            FailOutcomes = failOutcomes ?? new List<EventOutcomeEntry>();
        }
#endif
    }
}

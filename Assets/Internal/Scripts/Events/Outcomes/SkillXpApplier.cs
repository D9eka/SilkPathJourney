using System;
using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Player.Skills;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class SkillXpApplier : IOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.AddSkillXp
        };

        private readonly PlayerSkillRepository _skillRepository;

        public SkillXpApplier(PlayerSkillRepository skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            if (!Enum.TryParse(entry.Param, true, out SkillType skillType) || skillType == SkillType.None)
            {
                Debug.LogWarning($"[SPJ Events] Unknown skill param '{entry.Param}'");
                return;
            }

            int amount = Mathf.RoundToInt(entry.Value);
            _skillRepository.AddSkill(skillType, amount);
        }
    }
}

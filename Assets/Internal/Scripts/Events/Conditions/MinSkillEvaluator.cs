using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Player.Skills;
using UnityEngine;

namespace Internal.Scripts.Events.Conditions
{
    public class MinSkillEvaluator : IConditionEvaluator
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.MinSkill
        };

        private readonly PlayerSkillRepository _skillRepository;

        public MinSkillEvaluator(PlayerSkillRepository skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            if (!Enum.TryParse(condition.Param, true, out SkillType skillType) || skillType == SkillType.None)
            {
                Debug.LogWarning($"[SPJ Events] Unknown skill param '{condition.Param}'");
                return true;
            }

            int playerSkill = _skillRepository.Current.GetSkill(skillType);
            return playerSkill >= condition.Value;
        }
    }
}

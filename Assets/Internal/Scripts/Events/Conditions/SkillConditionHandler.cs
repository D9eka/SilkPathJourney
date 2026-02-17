using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using UnityEngine;

namespace Internal.Scripts.Events.Conditions
{
    public class SkillConditionHandler : IConditionHandler
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.MinSkill
        };

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            Debug.LogWarning($"[SPJ Events] MinSkill НЕ РЕАЛИЗОВАНО (param={condition.Param}, value={condition.Value})");
            return true;
        }
    }
}

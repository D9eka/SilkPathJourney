using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using UnityEngine;

namespace Internal.Scripts.Events.Conditions
{
    public class ConditionEvaluator
    {
        private readonly Dictionary<EventConditionType, IConditionHandler> _handlers = new();

        public ConditionEvaluator(
            ResourceConditionHandler resource,
            InventoryConditionHandler inventory,
            CartConditionHandler cart,
            LocationConditionHandler location,
            CompanionConditionHandler companion,
            SkillConditionHandler skill)
        {
            Register(resource);
            Register(inventory);
            Register(cart);
            Register(location);
            Register(companion);
            Register(skill);
        }

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            if (_handlers.TryGetValue(condition.Type, out var handler))
                return handler.Evaluate(condition, resources);

            Debug.LogWarning($"[SPJ Events] No condition handler for {condition.Type}");
            return true;
        }

        private void Register(IConditionHandler handler)
        {
            foreach (var type in handler.SupportedTypes)
                _handlers[type] = handler;
        }
    }
}

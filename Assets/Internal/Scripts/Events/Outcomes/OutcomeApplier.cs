using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.UI.Components;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class OutcomeApplier
    {
        private readonly Dictionary<EventOutcomeType, IOutcomeHandler> _handlers = new();

        public OutcomeApplier(
            ResourceOutcomeHandler resource,
            ItemOutcomeHandler item,
            CartDurabilityOutcomeHandler cartDurability)
        {
            Register(resource);
            Register(item);
            Register(cartDurability);
        }

        public ResourceType? GetAffectedResource(EventOutcomeType type)
        {
            if (_handlers.TryGetValue(type, out var handler))
                return handler.GetAffectedResource(type);
            return null;
        }

        public void Apply(EventOutcomeEntry entry)
        {
            if (_handlers.TryGetValue(entry.Type, out var handler))
            {
                handler.Apply(entry);
                return;
            }

            Debug.LogWarning($"[SPJ Events] No outcome handler for {entry.Type}");
        }

        private void Register(IOutcomeHandler handler)
        {
            foreach (var type in handler.SupportedTypes)
                _handlers[type] = handler;
        }
    }
}

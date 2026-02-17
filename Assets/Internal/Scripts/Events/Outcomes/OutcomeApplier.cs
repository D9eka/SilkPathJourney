using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
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
            CartDurabilityOutcomeHandler cartDurability,
            CompanionOutcomeHandler companion,
            SkillOutcomeHandler skill,
            RoadUnlockOutcomeHandler roadUnlock)
        {
            Register(resource);
            Register(item);
            Register(cartDurability);
            Register(companion);
            Register(skill);
            Register(roadUnlock);
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

        public bool CanAffordAll(List<EventOutcomeEntry> outcomes)
        {
            if (outcomes == null || outcomes.Count == 0) return true;

            Dictionary<EventOutcomeType, float> net = new();
            foreach (var e in outcomes)
            {
                if (e.Type == EventOutcomeType.None) continue;
                net.TryGetValue(e.Type, out float cur);
                net[e.Type] = cur + e.Value;
            }

            foreach (var kvp in net)
            {
                if (kvp.Value >= 0) continue;
                if (_handlers.TryGetValue(kvp.Key, out var handler) && !handler.CanAfford(kvp.Key, kvp.Value))
                    return false;
            }
            return true;
        }

        private void Register(IOutcomeHandler handler)
        {
            foreach (var type in handler.SupportedTypes)
                _handlers[type] = handler;
        }
    }
}

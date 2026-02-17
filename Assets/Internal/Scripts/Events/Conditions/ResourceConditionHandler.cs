using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;

namespace Internal.Scripts.Events.Conditions
{
    public class ResourceConditionHandler : IConditionHandler
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.MinMoney, EventConditionType.MaxMoney,
            EventConditionType.MinFood, EventConditionType.MaxFood,
            EventConditionType.MinDanger, EventConditionType.MaxDanger
        };

        private static readonly Dictionary<EventConditionType, Func<PlayerResourceState, float>> Getters = new()
        {
            [EventConditionType.MinMoney] = r => r.Money,
            [EventConditionType.MaxMoney] = r => r.Money,
            [EventConditionType.MinFood] = r => r.Food,
            [EventConditionType.MaxFood] = r => r.Food,
            [EventConditionType.MinDanger] = r => r.AccumulatedDanger,
            [EventConditionType.MaxDanger] = r => r.AccumulatedDanger
        };

        private static readonly HashSet<EventConditionType> MinTypes = new()
        {
            EventConditionType.MinMoney,
            EventConditionType.MinFood,
            EventConditionType.MinDanger
        };

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            if (!Getters.TryGetValue(condition.Type, out var getter))
                return true;

            float current = getter(resources);
            return MinTypes.Contains(condition.Type)
                ? current >= condition.Value
                : current <= condition.Value;
        }
    }
}

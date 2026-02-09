using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;

namespace Internal.Scripts.Events.Conditions
{
    public class CartConditionHandler : IConditionHandler
    {
        private static readonly EventConditionType[] Types =
        {
            EventConditionType.MinCartDurability,
            EventConditionType.MaxCartDurability
        };

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            if (condition.Type == EventConditionType.MinCartDurability)
            {
                return resources.PlayerCart.Durability >= condition.Value
                    && (resources.Carts == null || resources.Carts.All(c => c.Durability >= condition.Value));
            }

            return resources.PlayerCart.Durability <= condition.Value
                || (resources.Carts != null && resources.Carts.Any(c => c.Durability <= condition.Value));
        }
    }
}

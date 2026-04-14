using Internal.Scripts.Events.Generated;
using Internal.Scripts.UI.Components;

namespace Internal.Scripts.Events.Outcomes
{
    public static class EventOutcomeTypeExtensions
    {
        public static ResourceType? ToResourceType(this EventOutcomeType type) => type switch
        {
            EventOutcomeType.Money => ResourceType.Money,
            EventOutcomeType.Food => ResourceType.Food,
            EventOutcomeType.Morale => ResourceType.Morale,
            EventOutcomeType.Danger => ResourceType.Danger,
            EventOutcomeType.CartDurability => ResourceType.PlayerCartDurability,
            EventOutcomeType.MainCartDurability => ResourceType.PlayerCartDurability,
            EventOutcomeType.ExtraCartDurability => ResourceType.OtherCartsDurability,
            EventOutcomeType.Reputation => ResourceType.Reputation,
            _ => null
        };
    }
}

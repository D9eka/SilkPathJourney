using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.UI.Components;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class CartDurabilityApplier : IOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.CartDurability
        };

        private readonly PlayerResourceRepository _resourceRepository;

        public CartDurabilityApplier(PlayerResourceRepository resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public ResourceType? GetAffectedResource(EventOutcomeType type) =>
            type == EventOutcomeType.CartDurability ? ResourceType.PlayerCartDurability : null;

        public void Apply(EventOutcomeEntry entry)
        {
            int cartIndex = string.IsNullOrEmpty(entry.Param) ? -1 : int.Parse(entry.Param);
            float durabilityChange = entry.Value;

            _resourceRepository.UpdateResources(s =>
            {
                if (cartIndex == -1)
                {
                    s.PlayerCart.Durability = Mathf.Clamp(
                        s.PlayerCart.Durability + durabilityChange, 0f, s.PlayerCart.MaxDurability);
                    foreach (var cart in s.Carts)
                        cart.Durability = Mathf.Clamp(cart.Durability + durabilityChange, 0f, cart.MaxDurability);
                }
                else if (cartIndex >= 0 && cartIndex < s.Carts.Count)
                {
                    var cart = s.Carts[cartIndex];
                    cart.Durability = Mathf.Clamp(cart.Durability + durabilityChange, 0f, cart.MaxDurability);
                }
            });
        }

        public bool CanAfford(EventOutcomeType type, float netValue) => true;
    }
}

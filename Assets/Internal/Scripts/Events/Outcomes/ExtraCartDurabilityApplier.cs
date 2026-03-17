using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class ExtraCartDurabilityApplier : IOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types = { EventOutcomeType.ExtraCartDurability };

        private readonly PlayerResourceRepository _resourceRepository;

        public ExtraCartDurabilityApplier(PlayerResourceRepository resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            float durabilityChange = entry.Value;

            _resourceRepository.UpdateResources(s =>
            {
                foreach (var cart in s.Carts)
                    cart.Durability = Mathf.Clamp(cart.Durability + durabilityChange, 0f, cart.MaxDurability);
            });
        }
    }
}

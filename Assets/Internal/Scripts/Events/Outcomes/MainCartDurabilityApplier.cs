using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class MainCartDurabilityApplier : IOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types = { EventOutcomeType.MainCartDurability };

        private readonly PlayerResourceRepository _resourceRepository;

        public MainCartDurabilityApplier(PlayerResourceRepository resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            float durabilityChange = entry.Value;

            _resourceRepository.UpdateResources(s =>
            {
                s.PlayerCart.Durability = Mathf.Clamp(
                    s.PlayerCart.Durability + durabilityChange, 0f, s.PlayerCart.MaxDurability);
            });
        }
    }
}

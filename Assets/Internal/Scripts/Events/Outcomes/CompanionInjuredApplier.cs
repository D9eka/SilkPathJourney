using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;

namespace Internal.Scripts.Events.Outcomes
{
    public class CompanionInjuredApplier : IOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types = { EventOutcomeType.CompanionInjured };

        private readonly PlayerResourceRepository _resourceRepository;

        public CompanionInjuredApplier(PlayerResourceRepository resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            _resourceRepository.UpdateResources(s =>
            {
                if (s.Companions == null)
                    return;

                CompanionState target = string.IsNullOrEmpty(entry.Param)
                    ? s.Companions.FirstOrDefault(c => !c.IsInjured)
                    : s.Companions.FirstOrDefault(c =>
                        string.Equals(c.TypeId, entry.Param, StringComparison.OrdinalIgnoreCase) && !c.IsInjured);

                if (target != null)
                    target.IsInjured = true;
            });
        }
    }
}

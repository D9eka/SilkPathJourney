using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;

namespace Internal.Scripts.Events.Outcomes
{
    public class ReputationApplier : IOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types = { EventOutcomeType.Reputation };

        private readonly PlayerResourceRepository _resourceRepo;

        public ReputationApplier(PlayerResourceRepository resourceRepo)
        {
            _resourceRepo = resourceRepo;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            _resourceRepo.UpdateResources(s =>
                s.Reputation = Math.Clamp(s.Reputation + (int)entry.Value, 0, 100));
        }
    }
}

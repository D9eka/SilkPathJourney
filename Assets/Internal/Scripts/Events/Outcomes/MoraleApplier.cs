using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class MoraleApplier : IOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types = { EventOutcomeType.Morale };

        private readonly PlayerResourceRepository _resourceRepo;

        public MoraleApplier(PlayerResourceRepository resourceRepo)
        {
            _resourceRepo = resourceRepo;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            _resourceRepo.UpdateResources(s =>
                s.Morale = Mathf.Clamp(s.Morale + entry.Value, PlayerResourceState.MORALE_MIN, PlayerResourceState.MORALE_MAX));
        }
    }
}

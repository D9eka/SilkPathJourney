using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.UI.Components;
namespace Internal.Scripts.Events.Outcomes
{
    public class ReputationApplier : IResourceOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types = { EventOutcomeType.Reputation };

        private readonly PlayerResourceRepository _resourceRepo;

        public ReputationApplier(PlayerResourceRepository resourceRepo)
        {
            _resourceRepo = resourceRepo;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public ResourceType? GetAffectedResource(EventOutcomeType type) =>
            type == EventOutcomeType.Reputation ? ResourceType.Reputation : null;

        public void Apply(EventOutcomeEntry entry)
        {
            _resourceRepo.UpdateResources(s => s.Reputation += (int)entry.Value);
        }

        public bool CanAfford(EventOutcomeType type, float netValue) => true;
    }
}

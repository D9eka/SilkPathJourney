using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;

namespace Internal.Scripts.Events.Outcomes
{
    public class CompanionApplier : IOutcomeApplier
    {
        private const string DEFAULT_QUALITY = "experienced";

        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.AddCompanion
        };

        private readonly PlayerResourceRepository _resourceRepository;

        public CompanionApplier(PlayerResourceRepository resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            string typeId = entry.Param;
            string qualityId = entry.Value > 0 ? ((int)entry.Value).ToString() : DEFAULT_QUALITY;

            _resourceRepository.UpdateResources(s =>
            {
                if (s.Companions.Count >= s.MaxCompanions)
                    return;

                s.Companions.Add(new CompanionState
                {
                    TypeId = typeId,
                    QualityId = qualityId,
                    IsInjured = false
                });
            });
        }
    }
}

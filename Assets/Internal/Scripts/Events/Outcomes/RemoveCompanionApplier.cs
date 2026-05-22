using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;

namespace Internal.Scripts.Events.Outcomes
{
    public class RemoveCompanionApplier : IOutcomeApplier
    {
        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.RemoveCompanion
        };

        private readonly PlayerResourceRepository _resourceRepository;

        public RemoveCompanionApplier(PlayerResourceRepository resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            string typeId = entry.Param;
            if (string.IsNullOrEmpty(typeId)) return;

            _resourceRepository.UpdateResources(s =>
            {
                int idx = s.Companions.FindIndex(c =>
                    string.Equals(c.TypeId, typeId, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0)
                    s.Companions.RemoveAt(idx);
            });
        }
    }
}

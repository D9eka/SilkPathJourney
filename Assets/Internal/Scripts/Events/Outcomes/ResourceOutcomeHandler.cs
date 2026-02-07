using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Events.Data;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class ResourceOutcomeHandler : IOutcomeHandler
    {
        private static readonly EventOutcomeType[] Types =
        {
            EventOutcomeType.Money,
            EventOutcomeType.Food,
            EventOutcomeType.Danger
        };

        private readonly PlayerResourceRepository _resourceRepository;
        private readonly GameBalanceConfig _balanceConfig;

        public ResourceOutcomeHandler(PlayerResourceRepository resourceRepository,
            GameBalanceConfig balanceConfig)
        {
            _resourceRepository = resourceRepository;
            _balanceConfig = balanceConfig;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => Types;

        public void Apply(EventOutcomeEntry entry)
        {
            switch (entry.Type)
            {
                case EventOutcomeType.Money:
                    _resourceRepository.UpdateResources(s => s.Money += Mathf.RoundToInt(entry.Value));
                    break;
                case EventOutcomeType.Food:
                    _resourceRepository.UpdateResources(s =>
                        s.Food = Mathf.Clamp(s.Food + entry.Value, 0f, _balanceConfig.MaxFood));
                    break;
                case EventOutcomeType.Danger:
                    _resourceRepository.UpdateResources(s =>
                        s.AccumulatedDanger = Mathf.Max(0f, s.AccumulatedDanger + entry.Value));
                    break;
            }
        }
    }
}

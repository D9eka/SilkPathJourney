using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.State;
using Internal.Scripts.UI.Components;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class RoadUnlockOutcomeHandler : IOutcomeHandler
    {
        private readonly RoadUnlockService _unlockService;
        private readonly RoadNetwork _network;

        public RoadUnlockOutcomeHandler(RoadUnlockService unlockService, RoadNetwork network)
        {
            _unlockService = unlockService;
            _network = network;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => new[] { EventOutcomeType.UnlockRoad };

        public ResourceType? GetAffectedResource(EventOutcomeType type) => null;

        public bool CanAfford(EventOutcomeType type, float netValue) => true;

        public void Apply(EventOutcomeEntry entry)
        {
            string roadId = entry.Param;
            if (string.IsNullOrWhiteSpace(roadId))
            {
                Debug.LogWarning("[SPJ Events] UnlockRoad outcome has empty Param (roadId).");
                return;
            }

            _unlockService.UnlockRoad(roadId);
            _network.UnlockRoad(roadId);
        }
    }
}

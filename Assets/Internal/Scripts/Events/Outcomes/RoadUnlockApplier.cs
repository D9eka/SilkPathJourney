using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.State;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class RoadUnlockApplier : IOutcomeApplier
    {
        private readonly RoadUnlockService _unlockService;
        private readonly RoadNetwork _network;

        public RoadUnlockApplier(RoadUnlockService unlockService, RoadNetwork network)
        {
            _unlockService = unlockService;
            _network = network;
        }

        public IEnumerable<EventOutcomeType> SupportedTypes => new[] { EventOutcomeType.UnlockRoad };

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

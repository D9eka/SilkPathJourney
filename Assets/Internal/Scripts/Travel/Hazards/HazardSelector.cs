using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Player;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards
{
    public sealed class HazardSelector
    {
        private readonly HazardDatabase _database;
        private readonly IPlayerStateProvider _playerState;
        private readonly ICityNodeResolver _cityNodeResolver;

        public HazardSelector(
            HazardDatabase database,
            IPlayerStateProvider playerState,
            ICityNodeResolver cityNodeResolver)
        {
            _database = database;
            _playerState = playerState;
            _cityNodeResolver = cityNodeResolver;
        }

        public HazardData SelectHazard()
        {
            if (_database == null || _database.Hazards == null || _database.Hazards.Count == 0)
                return null;

            Biome currentBiome = _cityNodeResolver.ResolveRoadBiome(
                _playerState.CurrentFromNodeId, _playerState.CurrentToNodeId);

            var eligible = new System.Collections.Generic.List<HazardData>();
            foreach (HazardData hazard in _database.Hazards)
            {
                if (hazard.BiomeFilter == Biome.Unknown || hazard.BiomeFilter == currentBiome)
                    eligible.Add(hazard);
            }

            if (eligible.Count == 0)
                return null;

            return eligible[Random.Range(0, eligible.Count)];
        }
    }
}

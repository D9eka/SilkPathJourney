using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Events;
using Internal.Scripts.Road.Core;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.WorldModifiers
{
    public sealed class WorldModifierSpawner : IInitializable, IDisposable
    {
        private const float CitySpawnChance = 0.05f;
        private const float RoadSpawnChance = 0.20f;
        private const int MaxCityModifiers = 2;
        private const int MaxRoadModifiers = 2;

        private readonly DayTracker _dayTracker;
        private readonly WorldModifierRepository _repo;
        private readonly EconomyDatabase _economyDb;
        private readonly RoadRuntime[] _roads;
        private readonly ICityNodeResolver _cityResolver;
        private readonly GameBalanceConfig _balanceConfig;

        public WorldModifierSpawner(
            DayTracker dayTracker,
            WorldModifierRepository repo,
            EconomyDatabase economyDb,
            RoadRuntime[] roads,
            ICityNodeResolver cityResolver,
            GameBalanceConfig balanceConfig)
        {
            _dayTracker = dayTracker;
            _repo = repo;
            _economyDb = economyDb;
            _roads = roads;
            _cityResolver = cityResolver;
            _balanceConfig = balanceConfig;
        }

        public void Initialize()
        {
            _dayTracker.OnDayChanged += HandleDayChanged;
        }

        public void Dispose()
        {
            _dayTracker.OnDayChanged -= HandleDayChanged;
        }

        private void HandleDayChanged(int day)
        {
            _repo.ClearDayLog();
            _repo.RemoveExpired(day);
            TrySpawnCityModifiers(day);
            TrySpawnRoadModifiers(day);
            TryCascadeToRoads(day);
        }

        private void TrySpawnCityModifiers(int day)
        {
            foreach (CityData city in _economyDb.Cities)
            {
                if (_repo.GetCityModifierCount(city.Id) >= MaxCityModifiers) continue;
                if (UnityEngine.Random.value > CitySpawnChance) continue;

                TrySpawnCityModifier(city.Id, city.Biome, day);
            }
        }

        private void TrySpawnCityModifier(string cityId, Biome biome, int day)
        {
            var candidates = _economyDb.CityModifiers
                .Where(m => m.GetSpawnWeight(biome) > 0
                    && !_repo.HasCityConflict(cityId, m.ConflictGroup, _economyDb)
                    && !_repo.GetCityModifiers(cityId).Any(a => a.ModifierId == m.Id))
                .ToList();

            if (candidates.Count == 0) return;

            var modifier = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            int duration = (int)(modifier.RollDuration() * _balanceConfig.ModifierDurationMultiplier);
            _repo.AddCityModifier(cityId, modifier.Id, day, duration);
        }

        private void TrySpawnRoadModifiers(int day)
        {
            foreach (RoadRuntime road in _roads)
            {
                if (road?.Data == null) continue;
                string roadId = road.Data.RoadId;

                if (_repo.GetRoadModifierCount(roadId) >= MaxRoadModifiers) continue;
                if (UnityEngine.Random.value > RoadSpawnChance) continue;

                TrySpawnRoadModifier(roadId, _cityResolver.ResolveRoadBiome(road.Data.StartNodeId, road.Data.EndNodeId), day);
            }
        }

        private void TrySpawnRoadModifier(string roadId, Biome biome, int day)
        {
            var candidates = _economyDb.RoadModifiers
                .Where(m => m.GetSpawnWeight(biome) > 0
                    && !_repo.GetRoadModifiers(roadId).Any(a => a.ModifierId == m.Id))
                .ToList();

            if (candidates.Count == 0) return;

            var modifier = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            int duration = (int)(modifier.RollDuration() * _balanceConfig.ModifierDurationMultiplier);
            _repo.AddRoadModifier(roadId, modifier.Id, day, duration);
        }

        private void TryCascadeToRoads(int day)
        {
            foreach (CityData city in _economyDb.Cities)
            {
                var activeMods = _repo.GetCityModifiers(city.Id);
                foreach (var entry in activeMods)
                {
                    var modData = _economyDb.GetCityModifier(entry.ModifierId);
                    if (modData == null || string.IsNullOrEmpty(modData.CascadeRoadId)) continue;
                    if (modData.CascadeChance <= 0f) continue;

                    var roadModData = _economyDb.GetRoadModifier(modData.CascadeRoadId);
                    if (roadModData == null) continue;

                    foreach (var road in _roads)
                    {
                        if (road?.Data == null) continue;
                        var data = road.Data;
                        if (data.StartNodeId != city.NodeId && data.EndNodeId != city.NodeId) continue;

                        string roadId = data.RoadId;
                        if (_repo.GetRoadModifierCount(roadId) >= MaxRoadModifiers) continue;
                        if (_repo.GetRoadModifiers(roadId).Any(m => m.ModifierId == modData.CascadeRoadId)) continue;
                        if (UnityEngine.Random.value > modData.CascadeChance) continue;

                        int cascadeDur = (int)(roadModData.RollDuration() * _balanceConfig.ModifierDurationMultiplier);
                        _repo.AddRoadModifier(roadId, modData.CascadeRoadId, day, cascadeDur,
                            ModifierChangeReason.CascadeFrom);
                    }
                }
            }
        }
    }
}

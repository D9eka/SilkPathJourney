using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Npc.Trading;
using UnityEngine;

namespace Internal.Scripts.Npc.Routing
{
    public sealed class NpcRouteDecisionService
    {
        private readonly NpcSimulationSettings _settings;
        private readonly NpcSupplyPlanner _supplyPlanner;
        private readonly NpcKnowledgeService _knowledgeService;
        private readonly NpcTrader _trader;
        private readonly List<(string nodeId, float score)> _candidates = new();

        public NpcRouteDecisionService(
            NpcSimulationSettings settings,
            NpcSupplyPlanner supplyPlanner,
            NpcKnowledgeService knowledgeService,
            NpcTrader trader)
        {
            _settings = settings;
            _supplyPlanner = supplyPlanner;
            _knowledgeService = knowledgeService;
            _trader = trader;
        }

        public NpcRouteDecisionResult ChooseNextTarget(
            NpcRouteDecisionContext context,
            INpcRouteDecisionEnvironment environment,
            Func<float> nextRandomValue)
        {
            if (environment == null || environment.CityNodeIds == null || environment.CityNodeIds.Count == 0)
                return default;

            string nearestFallback = environment.FindNearestCityNode(context.CurrentNodeId);
            if (_settings.Archetypes == null || _settings.Archetypes.Count == 0)
                return WrapFallback(nearestFallback, context, environment);

            NpcArchetypeDefinition archDef = _settings.GetArchetypeDefinition(context.EconomyState.Archetype);

            string forced = TryForceContractDestination(context, environment);
            if (forced != null)
                return new NpcRouteDecisionResult(forced, NpcRouteFallbackKind.None);

            bool hasTradeCargo = _trader.HasTradeCargo(context.EconomyState);
            _candidates.Clear();
            int knowledgeDuration = _knowledgeService.GetKnowledgeDuration(context.EconomyState.Experience);

            foreach (string nodeId in environment.CityNodeIds)
            {
                if (nodeId == context.CurrentNodeId)
                    continue;
                if (!environment.TryGetCityByNodeId(nodeId, out CityData targetCity))
                    continue;
                if (!_supplyPlanner.CanAffordTrip(context.EconomyState, context.CurrentCityId,
                        context.CurrentNodeId, nodeId, context.SpeedMetersPerDay))
                    continue;

                var (excluded, modBonus) = EvaluateModifiers(
                    context.EconomyState.Knowledge, knowledgeDuration, targetCity.Id, context.CurrentDay,
                    archDef, context.EconomyState.Archetype, nextRandomValue);
                if (excluded)
                    continue;

                float score = CalculateCityScore(context, environment, targetCity, nodeId, archDef, hasTradeCargo, modBonus);
                if (score > 0f)
                    _candidates.Add((nodeId, score));
            }

            if (_candidates.Count > 0)
                return new NpcRouteDecisionResult(
                    WeightedRandomSelect(_candidates, nextRandomValue),
                    NpcRouteFallbackKind.None);

            return WrapFallback(nearestFallback, context, environment);
        }

        private string TryForceContractDestination(
            NpcRouteDecisionContext context,
            INpcRouteDecisionEnvironment environment)
        {
            if (!context.EconomyState.ActiveContract.HasValue)
                return null;

            string contractTargetCityId = context.EconomyState.ActiveContract.Value.TargetCityId;
            foreach (string nodeId in environment.CityNodeIds)
            {
                if (!environment.TryGetCityByNodeId(nodeId, out CityData contractCity))
                    continue;
                if (contractCity.Id != contractTargetCityId)
                    continue;
                if (_supplyPlanner.CanAffordTrip(context.EconomyState, context.CurrentCityId,
                        context.CurrentNodeId, nodeId, context.SpeedMetersPerDay))
                    return nodeId;
                break;
            }

            return null;
        }

        private (bool excluded, float bonus) EvaluateModifiers(
            NpcKnowledgeState knowledge, int duration, string cityId, int currentDay,
            NpcArchetypeDefinition archDef,
            NpcArchetype archetype,
            Func<float> nextRandomValue)
        {
            bool excluded = false;
            float modifierBonus = 0f;

            foreach (KnownCityModifier mod in knowledge.Entries)
            {
                if (mod.CityId != cityId || mod.LearnedDay + duration <= currentDay)
                    continue;
                if (IsExclusionModifier(mod.ModifierId))
                {
                    if (NextRandomValue(nextRandomValue) > archDef.WarEntryChance)
                    {
                        excluded = true;
                        break;
                    }
                }

                if (mod.ModifierId == CityModifierId.Festival)
                {
                    modifierBonus += archetype == NpcArchetype.BazaarTrader
                        ? _settings.BazaarFestivalScoreBonus
                        : _settings.FestivalScoreBonus;
                }

                if (mod.ModifierId == CityModifierId.Bandits
                    && archetype != NpcArchetype.Adventurer
                    && archetype != NpcArchetype.SteppeHerder)
                {
                    modifierBonus -= _settings.BanditsScorePenalty;
                }

                if (mod.ModifierId == CityModifierId.Drought && archetype == NpcArchetype.BazaarTrader)
                    modifierBonus += _settings.DroughtBazaarBonus;

                if (mod.ModifierId == CityModifierId.HighTaxes)
                    modifierBonus -= _settings.HighTaxesPenalty;

                if (mod.ModifierId == CityModifierId.CaravanArrival)
                    modifierBonus += _settings.CaravanArrivalBonus;
            }

            return (excluded, modifierBonus);
        }

        private float CalculateCityScore(
            NpcRouteDecisionContext context,
            INpcRouteDecisionEnvironment environment,
            CityData targetCity,
            string nodeId,
            NpcArchetypeDefinition archDef,
            bool hasTradeCargo,
            float modifierBonus)
        {
            float tradePotential = hasTradeCargo
                ? _trader.EstimateHeldCargoTradePotential(
                    context.EconomyState,
                    context.CurrentCityId,
                    context.CurrentNodeId,
                    targetCity.Id,
                    nodeId,
                    context.SpeedMetersPerDay)
                : _trader.EstimateBootstrapTradePotential(
                    context.EconomyState,
                    context.CurrentCityId,
                    context.CurrentNodeId,
                    nodeId,
                    context.SpeedMetersPerDay);

            float affinity = GetCityAffinity(context.EconomyState.Archetype, targetCity.Type);
            float estimatedDays = environment.EstimateTravelDays(
                context.CurrentNodeId, nodeId, context.SpeedMetersPerDay);
            float adjustedDays = ApplyRouteDurationBias(estimatedDays, archDef.RouteDurationBias);
            float distancePenalty = 1f + adjustedDays * 0.02f;
            float score = (tradePotential * affinity + modifierBonus) / distancePenalty;

            if (context.EconomyState.ActiveContract.HasValue
                && context.EconomyState.ActiveContract.Value.TargetCityId == targetCity.Id)
            {
                score *= 3.0f;
            }

            return score;
        }

        private NpcRouteDecisionResult WrapFallback(
            string nearestFallback,
            NpcRouteDecisionContext context,
            INpcRouteDecisionEnvironment environment)
        {
            if (!string.IsNullOrEmpty(nearestFallback))
                return new NpcRouteDecisionResult(nearestFallback, NpcRouteFallbackKind.NearestCity);

            foreach (string nodeId in environment.CityNodeIds)
            {
                if (nodeId != context.CurrentNodeId)
                    return new NpcRouteDecisionResult(nodeId, NpcRouteFallbackKind.EmergencyCity);
            }

            return default;
        }

        private float GetCityAffinity(NpcArchetype archetype, CityType cityType)
        {
            if (_settings.CityAffinityTable == null)
                return 1f;

            foreach (ArchetypeCityAffinity entry in _settings.CityAffinityTable)
            {
                if (entry.Archetype == archetype && entry.CityType == cityType)
                    return entry.Affinity;
            }

            return 1f;
        }

        private static bool IsExclusionModifier(string modifierId)
        {
            return modifierId == CityModifierId.War || modifierId == CityModifierId.Epidemic;
        }

        private static float ApplyRouteDurationBias(float estimatedDays, float routeDurationBias)
        {
            float multiplier = Mathf.Clamp(1f - routeDurationBias, 0.6f, 1.6f);
            return Mathf.Max(1f, estimatedDays * multiplier);
        }

        private static string WeightedRandomSelect(List<(string nodeId, float score)> candidates, Func<float> nextRandomValue)
        {
            if (candidates.Count == 0)
                return null;

            float total = 0f;
            foreach ((string _, float score) in candidates)
                total += score;

            float roll = NextRandomValue(nextRandomValue) * total;
            float cumulative = 0f;
            foreach ((string nodeId, float score) in candidates)
            {
                cumulative += score;
                if (roll <= cumulative)
                    return nodeId;
            }

            return candidates[candidates.Count - 1].nodeId;
        }

        private static float NextRandomValue(Func<float> nextRandomValue)
        {
            if (nextRandomValue == null)
                return UnityEngine.Random.value;

            return Mathf.Clamp01(nextRandomValue());
        }
    }
}

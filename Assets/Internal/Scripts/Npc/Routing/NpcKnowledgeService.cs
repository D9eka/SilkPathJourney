using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Events;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Npc.Save;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.WorldModifiers;

namespace Internal.Scripts.Npc.Routing
{
    public sealed class NpcKnowledgeService
    {
        private readonly WorldModifierRepository _modifierRepository;
        private readonly CityRadiusService _radiusResolver;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly DayTracker _dayTracker;
        private readonly NpcSimulationSettings _settings;

        public NpcKnowledgeService(
            WorldModifierRepository modifierRepository,
            CityRadiusService radiusResolver,
            ICityNodeResolver cityNodeResolver,
            DayTracker dayTracker,
            NpcSimulationSettings settings)
        {
            _modifierRepository = modifierRepository;
            _radiusResolver = radiusResolver;
            _cityNodeResolver = cityNodeResolver;
            _dayTracker = dayTracker;
            _settings = settings;
        }

        public void LearnFromVisit(NpcAgentSaveState agent, string cityNodeId)
            => LearnFromVisitInternal(agent.Knowledge, cityNodeId);

        public void LearnFromVisit(NpcEconomyState agent, string cityNodeId)
            => LearnFromVisitInternal(agent.Knowledge, cityNodeId);

        private void LearnFromVisitInternal(NpcKnowledgeState knowledge, string cityNodeId)
        {
            if (!_cityNodeResolver.TryGetCityByNodeId(cityNodeId, out CityData city))
                return;

            int currentDay = _dayTracker.CurrentDay;

            LearnCityModifiers(knowledge, city.Id, currentDay);

            bool hasTavern = System.Array.IndexOf(city.Buildings, BuildingId.Tavern) >= 0;
            if (hasTavern)
            {
                List<CityData> nearby = _radiusResolver.GetCitiesInRadius(cityNodeId, 2);
                foreach (CityData nearbyCity in nearby)
                    LearnCityModifiers(knowledge, nearbyCity.Id, currentDay);
            }
        }

        public List<KnownCityModifier> GetKnownModifiers(NpcAgentSaveState agent, string cityId, int currentDay)
            => GetKnownModifiers(agent.Knowledge, agent.Experience, cityId, currentDay);

        public List<KnownCityModifier> GetKnownModifiers(NpcEconomyState agent, string cityId, int currentDay)
            => GetKnownModifiers(agent.Knowledge, agent.Experience, cityId, currentDay);

        private List<KnownCityModifier> GetKnownModifiers(NpcKnowledgeState knowledge, NpcExperienceLevel experience,
            string cityId, int currentDay)
        {
            float mult = GetExperienceMult(experience);
            int duration = UnityEngine.Mathf.RoundToInt(_settings.BaseKnowledgeDuration * mult);

            var result = new List<KnownCityModifier>();
            foreach (KnownCityModifier entry in knowledge.Entries)
            {
                if (entry.CityId != cityId)
                    continue;
                if (entry.LearnedDay + duration > currentDay)
                    result.Add(entry);
            }
            return result;
        }

        public void PruneExpired(NpcAgentSaveState agent, int currentDay)
            => PruneExpiredInternal(agent.Knowledge, agent.Experience, currentDay);

        public void PruneExpired(NpcEconomyState agent, int currentDay)
            => PruneExpiredInternal(agent.Knowledge, agent.Experience, currentDay);

        private void PruneExpiredInternal(NpcKnowledgeState knowledge, NpcExperienceLevel experience, int currentDay)
        {
            float mult = GetExperienceMult(experience);
            int duration = UnityEngine.Mathf.RoundToInt(_settings.BaseKnowledgeDuration * mult);
            knowledge.Entries.RemoveAll(e => e.LearnedDay + duration <= currentDay);
        }

        private void LearnCityModifiers(NpcKnowledgeState knowledge, string cityId, int currentDay)
        {
            List<ActiveModifierEntry> modifiers = _modifierRepository.GetCityModifiers(cityId);
            foreach (ActiveModifierEntry modifier in modifiers)
                AddOrUpdateEntry(knowledge, cityId, modifier.ModifierId, currentDay);
        }

        private void AddOrUpdateEntry(NpcKnowledgeState knowledge, string cityId, string modifierId, int learnedDay)
        {
            List<KnownCityModifier> entries = knowledge.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].CityId == cityId && entries[i].ModifierId == modifierId)
                {
                    entries[i] = new KnownCityModifier
                    {
                        CityId = cityId,
                        ModifierId = modifierId,
                        LearnedDay = learnedDay
                    };
                    return;
                }
            }
            entries.Add(new KnownCityModifier
            {
                CityId = cityId,
                ModifierId = modifierId,
                LearnedDay = learnedDay
            });
        }

        private float GetExperienceMult(NpcExperienceLevel experience)
        {
            return experience switch
            {
                NpcExperienceLevel.Experienced => _settings.ExperiencedKnowledgeMult,
                NpcExperienceLevel.Master => _settings.MasterKnowledgeMult,
                _ => _settings.NoviceKnowledgeMult
            };
        }
    }
}

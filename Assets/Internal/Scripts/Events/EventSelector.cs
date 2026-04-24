using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Events.Conditions;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Internal.Scripts.Events
{
    public class EventSelector
    {
#if UNITY_EDITOR
        public static string DebugEventPrefix { get; set; }
#endif
        private readonly EventDatabase _eventDatabase;
        private readonly ConditionEvaluator _conditionEvaluator;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly OutcomeApplier _outcomeApplier;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly IPlayerStateProvider _playerState;

        private readonly Queue<string> _recentEventIds = new();
        private const int RECENT_HISTORY_SIZE = 5;
        private const int MIN_AVAILABLE_CHOICES = 2;

        public EventSelector(
            EventDatabase eventDatabase,
            ConditionEvaluator conditionEvaluator,
            PlayerResourceRepository resourceRepository,
            OutcomeApplier outcomeApplier,
            ICityNodeResolver cityNodeResolver,
            IPlayerStateProvider playerState)
        {
            _eventDatabase = eventDatabase;
            _conditionEvaluator = conditionEvaluator;
            _resourceRepository = resourceRepository;
            _outcomeApplier = outcomeApplier;
            _cityNodeResolver = cityNodeResolver;
            _playerState = playerState;
        }

        public List<EventData> GetEligibleEvents(bool minor, Predicate<EventData> filter = null)
        {
            if (_eventDatabase?.Events == null) return new List<EventData>();

            Biome currentBiome = _cityNodeResolver.ResolveRoadBiome(
                _playerState.CurrentFromNodeId, _playerState.CurrentToNodeId);

            var eligible = new List<EventData>();
            foreach (var evt in _eventDatabase.Events)
            {
                if (IsEligible(evt, minor, currentBiome, filter))
                    eligible.Add(evt);
            }
            return eligible;
        }

        public EventData SelectEvent(bool minor, Predicate<EventData> filter = null)
        {
            var eligible = GetEligibleEvents(minor, filter);
            if (eligible.Count == 0)
                return null;

            float totalWeight = 0f;
            foreach (var evt in eligible)
                totalWeight += evt.Weight;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            foreach (var evt in eligible)
            {
                cumulative += evt.Weight;
                if (roll <= cumulative)
                    return evt;
            }

            return eligible[eligible.Count - 1];
        }

        private bool IsEligible(EventData evt, bool minor, Biome currentBiome, Predicate<EventData> filter = null)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(DebugEventPrefix) && !evt.Id.StartsWith(DebugEventPrefix))
                return false;
#endif
            if (evt.IsMinor != minor) return false;
            if (evt.Weight <= 0f) return false;
            Biome eventBiome = evt.Biome;
            if (eventBiome != Biome.Unknown && eventBiome != currentBiome) return false;
            if (!CheckConditions(evt.Conditions)) return false;
            if (filter != null && !filter(evt)) return false;
            if (_recentEventIds.Contains(evt.Id)) return false;
            if (!HasAvailableChoices(evt)) return false;
            return true;
        }

        public void RegisterRecentEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId)) return;
            _recentEventIds.Enqueue(eventId);
            while (_recentEventIds.Count > RECENT_HISTORY_SIZE)
                _recentEventIds.Dequeue();
        }

        public List<EventChoice> GetAvailableChoices(EventData eventData)
        {
            if (eventData?.Choices == null)
                return new List<EventChoice>();

            return eventData.Choices.Where(c =>
                (c.Conditions == null || c.Conditions.Count == 0 || CheckConditions(c.Conditions))
                && _outcomeApplier.CanAffordAll(c.Outcomes)).ToList();
        }

        public bool HasAvailableChoices(EventData eventData)
        {
            if (eventData?.Choices == null || eventData.Choices.Count < MIN_AVAILABLE_CHOICES)
                return false;
            int available = 0;
            foreach (var c in eventData.Choices)
            {
                if ((c.Conditions == null || c.Conditions.Count == 0 || CheckConditions(c.Conditions))
                    && _outcomeApplier.CanAffordAll(c.Outcomes))
                {
                    if (++available >= MIN_AVAILABLE_CHOICES) return true;
                }
            }
            return false;
        }

        public bool CheckConditions(List<EventCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0)
                return true;

            var resources = _resourceRepository.Current;
            return conditions.All(c => _conditionEvaluator.Evaluate(c, resources));
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events.Conditions;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.Player;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Save;
using Internal.Scripts.World.State;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.StackService;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Internal.Scripts.Events
{
    public class EventTrigger : IInitializable, IDisposable
    {
        private readonly DayTracker _dayTracker;
        private readonly EventDatabase _eventDatabase;
        private readonly ScreenStackService _screenStackService;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly SaveRepository _saveRepository;
        private readonly GameClock _gameClock;
        private readonly ConditionEvaluator _conditionEvaluator;
        private readonly OutcomeApplier _outcomeApplier;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly IRoadNodeLookup _nodeLookup;
        private readonly PlayerController _playerController;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly EventToastController _toastController;

        public EventTrigger(
            DayTracker dayTracker,
            EventDatabase eventDatabase,
            ScreenStackService screenStackService,
            PlayerResourceRepository resourceRepository,
            SaveRepository saveRepository,
            GameClock gameClock,
            ConditionEvaluator conditionEvaluator,
            OutcomeApplier outcomeApplier,
            GameBalanceConfig balanceConfig,
            IRoadNodeLookup nodeLookup,
            PlayerController playerController,
            ICityNodeResolver cityNodeResolver,
            EventToastController toastController)
        {
            _dayTracker = dayTracker;
            _eventDatabase = eventDatabase;
            _screenStackService = screenStackService;
            _resourceRepository = resourceRepository;
            _saveRepository = saveRepository;
            _gameClock = gameClock;
            _conditionEvaluator = conditionEvaluator;
            _outcomeApplier = outcomeApplier;
            _balanceConfig = balanceConfig;
            _nodeLookup = nodeLookup;
            _playerController = playerController;
            _cityNodeResolver = cityNodeResolver;
            _toastController = toastController;
        }

        public void Initialize()
        {
            _dayTracker.OnDayChanged += HandleDayChanged;
        }

        public void Dispose()
        {
            _dayTracker.OnDayChanged -= HandleDayChanged;
        }

        private void HandleDayChanged(int currentDay)
        {
            if (_screenStackService.IsOpen(ScreenId.Event))
                return;

            TryTriggerMajorEvent(currentDay);
            TryTriggerMinorEvent(currentDay);
        }

        private void TryTriggerMajorEvent(int currentDay)
        {
            int lastMajorDay = _saveRepository.Data.Player.LastEventDay;
            if (currentDay - lastMajorDay < _balanceConfig.DaysBetweenMajorEvents)
                return;

            EventData eventData = SelectEvent(minor: false);
            if (eventData == null)
                return;

            TriggerMajorEvent(eventData, currentDay);
        }

        private void TryTriggerMinorEvent(int currentDay)
        {
            int lastMinorDay = _saveRepository.Data.Player.LastMinorEventDay;
            if (currentDay - lastMinorDay < _balanceConfig.DaysBetweenMinorEvents)
                return;

            EventData eventData = SelectEvent(minor: true);
            if (eventData == null)
                return;

            TriggerMinorEvent(eventData, currentDay);
        }

        private EventData SelectEvent(bool minor)
        {
            if (_eventDatabase == null || _eventDatabase.Events == null || _eventDatabase.Events.Count == 0)
                return null;

            List<EventData> eligible = new();
            float totalWeight = 0f;

            foreach (var evt in _eventDatabase.Events)
            {
                if (evt.IsMinor != minor)
                    continue;

                if (!CheckConditions(evt.Conditions))
                    continue;

                eligible.Add(evt);
                totalWeight += evt.Weight;
            }

            if (eligible.Count == 0)
                return null;

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

        public bool CheckConditions(List<EventCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0)
                return true;

            var resources = _resourceRepository.Current;
            return conditions.All(c => _conditionEvaluator.Evaluate(c, resources));
        }

        private void TriggerMajorEvent(EventData eventData, int currentDay)
        {
            string nearestNodeId = _nodeLookup.FindNearestNodeId(_playerController.CurrentPosition);
            bool isAtCity = _playerController.CurrentNodeId == nearestNodeId;
            _cityNodeResolver.TryGetCityByNodeId(nearestNodeId, out var city);
            var args = new EventTriggerArgs(eventData, city, isAtCity);

            if (!_screenStackService.TryOpen(ScreenId.Event, args, out ScreenOpenResult result))
            {
                Debug.LogWarning($"[SPJ Events] Cannot open event screen: {result}");
                return;
            }

            _saveRepository.Data.Player.LastEventDay = currentDay;
            _saveRepository.Save();
            _gameClock.Pause();
        }

        private void TriggerMinorEvent(EventData eventData, int currentDay)
        {
            ApplyOutcome(eventData.AutoOutcomes);
            _toastController.ShowToast(eventData);

            _saveRepository.Data.Player.LastMinorEventDay = currentDay;
            _saveRepository.Save();
        }

        public ResourceType? GetAffectedResource(EventOutcomeType type) =>
            _outcomeApplier.GetAffectedResource(type);

        public bool CanAffordOutcomes(List<EventOutcomeEntry> outcomes) =>
            _outcomeApplier.CanAffordAll(outcomes);

        public int LastChoiceIndex { get; set; } = -1;

        public event Action OnEventClosed;

        public void OnEventCompleted()
        {
            _gameClock.Resume();
            OnEventClosed?.Invoke();
        }

        public void ApplyOutcome(List<EventOutcomeEntry> outcomes)
        {
            if (outcomes == null || outcomes.Count == 0)
                return;

            foreach (var entry in outcomes)
                _outcomeApplier.Apply(entry);
        }
    }
}

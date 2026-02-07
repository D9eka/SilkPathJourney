using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Events.Conditions;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.Player;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Save;
using Internal.Scripts.World.State;
using Internal.Scripts.UI.Screen.Config;
using Internal.Scripts.UI.StackService;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Events
{
    public class EventTrigger : IInitializable, System.IDisposable
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
            PlayerController playerController)
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

            int lastEventDay = _saveRepository.Data.Player.LastEventDay;

            if (currentDay - lastEventDay < _balanceConfig.DaysBetweenEvents)
                return;

            EventData eventData = SelectEvent();
            if (eventData == null)
                return;

            TriggerEvent(eventData, currentDay);
        }

        private EventData SelectEvent()
        {
            if (_eventDatabase == null || _eventDatabase.Events == null || _eventDatabase.Events.Count == 0)
                return null;

            List<EventData> eligible = new();
            float totalWeight = 0f;

            foreach (var evt in _eventDatabase.Events)
            {
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

        private void TriggerEvent(EventData eventData, int currentDay)
        {
            string nearestNodeId = _nodeLookup.FindNearestNodeId(_playerController.CurrentPosition);
            var args = new EventTriggerArgs(eventData, nearestNodeId);

            if (!_screenStackService.TryOpen(ScreenId.Event, args, out ScreenOpenResult result))
            {
                Debug.LogWarning($"[SPJ Events] Cannot open event screen: {result}");
                return;
            }

            _saveRepository.Data.Player.LastEventDay = currentDay;
            _saveRepository.Save();
            _gameClock.Pause();
        }

        public void OnEventCompleted()
        {
            _gameClock.Resume();
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

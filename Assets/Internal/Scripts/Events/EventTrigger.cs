using System;
using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy.Cities;
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

namespace Internal.Scripts.Events
{
    public class EventTrigger : IInitializable, IDisposable
    {
        private readonly DayTracker _dayTracker;
        private readonly EventSelector _eventSelector;
        private readonly ScreenStackService _screenStackService;
        private readonly SaveRepository _saveRepository;
        private readonly GameClock _gameClock;
        private readonly OutcomeApplier _outcomeApplier;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly IRoadNodeLookup _nodeLookup;
        private readonly PlayerController _playerController;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly EventToastController _toastController;

        public EventTrigger(
            DayTracker dayTracker,
            EventSelector eventSelector,
            ScreenStackService screenStackService,
            SaveRepository saveRepository,
            GameClock gameClock,
            OutcomeApplier outcomeApplier,
            GameBalanceConfig balanceConfig,
            IRoadNodeLookup nodeLookup,
            PlayerController playerController,
            ICityNodeResolver cityNodeResolver,
            EventToastController toastController)
        {
            _dayTracker = dayTracker;
            _eventSelector = eventSelector;
            _screenStackService = screenStackService;
            _saveRepository = saveRepository;
            _gameClock = gameClock;
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
            if (_dayTracker.IsSkipping) return;
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

            EventData eventData = _eventSelector.SelectEvent(minor: false);
            if (eventData == null)
                return;

            TriggerMajorEvent(eventData, currentDay);
        }

        private void TryTriggerMinorEvent(int currentDay)
        {
            int lastMinorDay = _saveRepository.Data.Player.LastMinorEventDay;
            if (currentDay - lastMinorDay < _balanceConfig.DaysBetweenMinorEvents)
                return;

            EventData eventData = _eventSelector.SelectEvent(minor: true);
            if (eventData == null)
                return;

            TriggerMinorEvent(eventData, currentDay);
        }

        public bool TriggerEvent(EventData eventData)
        {
            if (_screenStackService.IsOpen(ScreenId.Event)) return false;

            string nearestNodeId = _nodeLookup.FindNearestNodeId(_playerController.CurrentPosition);
            bool isAtCity = _playerController.CurrentNodeId == nearestNodeId;
            _cityNodeResolver.TryGetCityByNodeId(nearestNodeId, out var city);
            var args = new EventTriggerArgs(eventData, city, isAtCity);

            if (!_screenStackService.TryOpen(ScreenId.Event, args, out ScreenOpenResult result))
            {
                Debug.LogWarning($"[SPJ Events] Cannot open event screen: {result}");
                return false;
            }

            _gameClock.Pause();
            _eventSelector.RegisterRecentEvent(eventData.Id);
            return true;
        }

        private void TriggerMajorEvent(EventData eventData, int currentDay)
        {
            if (!TriggerEvent(eventData)) return;
            _saveRepository.Data.Player.LastEventDay = currentDay;
            _saveRepository.Save();
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

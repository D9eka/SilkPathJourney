using System;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Player;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.StackService;
using Zenject;

namespace Internal.Scripts.Camp
{
    public sealed class CampController : IInitializable, IDisposable
    {
        private readonly CampActionService _campActionService;
        private readonly EventSelector _eventSelector;
        private readonly EventTrigger _eventTrigger;
        private readonly DayTracker _dayTracker;
        private readonly IPlayerStateEvents _playerStateEvents;
        private readonly ScreenStackService _screenStackService;

        public CampController(
            CampActionService campActionService,
            EventSelector eventSelector,
            EventTrigger eventTrigger,
            DayTracker dayTracker,
            IPlayerStateEvents playerStateEvents,
            ScreenStackService screenStackService)
        {
            _campActionService = campActionService;
            _eventSelector = eventSelector;
            _eventTrigger = eventTrigger;
            _dayTracker = dayTracker;
            _playerStateEvents = playerStateEvents;
            _screenStackService = screenStackService;
        }

        public void Initialize()
        {
            _playerStateEvents.OnCurrentNodeChanged += HandleNodeChanged;
        }

        public void Dispose()
        {
            _playerStateEvents.OnCurrentNodeChanged -= HandleNodeChanged;
        }

        public bool ExecuteActionAndAdvance(CampActionType type)
        {
            bool success = _campActionService.ExecuteAction(type);
            if (!success)
                return false;

            var sideEffect = _campActionService.GetSideEffectForRepeat(type);
            if (sideEffect.HasValue && sideEffect.Value.EventChance > 0)
            {
                if (UnityEngine.Random.value < sideEffect.Value.EventChance)
                    TryTriggerCampEvent();
            }
            else
            {
                TryTriggerCampEvent();
            }

            _dayTracker.AdvanceDays(1);
            return true;
        }

        private void TryTriggerCampEvent()
        {
            if (_screenStackService.IsOpen(ScreenId.Event))
                return;

            EventData eventData = _eventSelector.SelectEvent(minor: false);
            if (eventData == null)
                return;

            _eventTrigger.TriggerEvent(eventData);
        }

        private void HandleNodeChanged(string nodeId) => _campActionService.OnSegmentChanged();
    }
}

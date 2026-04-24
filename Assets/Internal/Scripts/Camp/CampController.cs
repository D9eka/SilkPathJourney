using System;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
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
            _eventTrigger.OnEventClosed += HandleEventClosed;
        }

        public void Dispose()
        {
            _playerStateEvents.OnCurrentNodeChanged -= HandleNodeChanged;
            _eventTrigger.OnEventClosed -= HandleEventClosed;
        }

        public bool ExecuteActionAndAdvance(CampActionType type)
        {
            bool success = _campActionService.ExecuteAction(type);
            if (!success)
                return false;

            bool opened = TryTriggerCampEvent(type);
            if (!opened)
            {
                _dayTracker.AdvanceDays(1);
                _campActionService.ClearCurrentAction();
            }
            return true;
        }

        private bool TryTriggerCampEvent(CampActionType actionType)
        {
            if (_screenStackService.IsOpen(ScreenId.Event)) return false;

            EventData eventData = _eventSelector.SelectEvent(
                minor: false,
                filter: evt => CampEventOutcomeScaler.ParseCampAction(evt) == actionType
                             || evt.Category == EventCategory.CampAny);

            if (eventData == null) return false;

            return _eventTrigger.TriggerEvent(eventData);
        }

        private void HandleEventClosed()
        {
            if (_campActionService.CurrentAction.HasValue)
            {
                _dayTracker.AdvanceDays(1);
                _campActionService.ClearCurrentAction();
            }
        }

        private void HandleNodeChanged(string nodeId) => _campActionService.OnSegmentChanged();
    }
}

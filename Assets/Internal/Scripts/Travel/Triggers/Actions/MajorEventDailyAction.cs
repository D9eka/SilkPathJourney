using Internal.Scripts.Config;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Save;

namespace Internal.Scripts.Travel.Triggers.Actions
{
    public sealed class MajorEventDailyAction : IDailyTriggerAction
    {
        private readonly EventTrigger _eventTrigger;
        private readonly EventSelector _eventSelector;
        private readonly EventQueueService _eventQueue;
        private readonly SaveRepository _saveRepository;
        private readonly GameBalanceConfig _balance;
        private readonly DayTracker _dayTracker;

        public MajorEventDailyAction(
            EventTrigger eventTrigger,
            EventSelector eventSelector,
            EventQueueService eventQueue,
            SaveRepository saveRepository,
            GameBalanceConfig balance,
            DayTracker dayTracker)
        {
            _eventTrigger = eventTrigger;
            _eventSelector = eventSelector;
            _eventQueue = eventQueue;
            _saveRepository = saveRepository;
            _balance = balance;
            _dayTracker = dayTracker;
        }

        public bool CanTrigger()
        {
            int currentDay = _dayTracker.CurrentDay;

            if (_eventQueue.TryPeek(currentDay) != null)
                return true;

            int lastMajorDay = _saveRepository.Data.Player.LastEventDay;
            if (currentDay - lastMajorDay < _balance.DaysBetweenMajorEvents)
                return false;

            return _eventSelector.SelectEvent(minor: false) != null;
        }

        public void Trigger()
        {
            int currentDay = _dayTracker.CurrentDay;

            EventData eventData = _eventQueue.TryDequeue(currentDay)
                ?? _eventSelector.SelectEvent(minor: false);

            if (eventData == null) return;
            _eventTrigger.TriggerMajorEvent(eventData, currentDay);
        }
    }
}

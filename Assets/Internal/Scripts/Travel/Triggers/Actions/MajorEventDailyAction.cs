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
        private readonly SaveRepository _saveRepository;
        private readonly GameBalanceConfig _balance;
        private readonly DayTracker _dayTracker;

        public MajorEventDailyAction(
            EventTrigger eventTrigger,
            EventSelector eventSelector,
            SaveRepository saveRepository,
            GameBalanceConfig balance,
            DayTracker dayTracker)
        {
            _eventTrigger = eventTrigger;
            _eventSelector = eventSelector;
            _saveRepository = saveRepository;
            _balance = balance;
            _dayTracker = dayTracker;
        }

        public bool CanTrigger()
        {
            int lastMajorDay = _saveRepository.Data.Player.LastEventDay;
            if (_dayTracker.CurrentDay - lastMajorDay < _balance.DaysBetweenMajorEvents)
                return false;

            return _eventSelector.SelectEvent(minor: false) != null;
        }

        public void Trigger()
        {
            EventData eventData = _eventSelector.SelectEvent(minor: false);
            if (eventData == null) return;
            _eventTrigger.TriggerMajorEvent(eventData, _dayTracker.CurrentDay);
        }
    }
}

using Internal.Scripts.Config;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Save;

namespace Internal.Scripts.Travel.Triggers.Actions
{
    public sealed class MinorEventDailyAction : IDailyTriggerAction
    {
        private readonly EventTrigger _eventTrigger;
        private readonly EventSelector _eventSelector;
        private readonly SaveRepository _saveRepository;
        private readonly GameBalanceConfig _balance;
        private readonly DayTracker _dayTracker;

        public MinorEventDailyAction(
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
            int lastMinorDay = _saveRepository.Data.Player.LastMinorEventDay;
            if (_dayTracker.CurrentDay - lastMinorDay < _balance.DaysBetweenMinorEvents)
                return false;

            return _eventSelector.SelectEvent(minor: true) != null;
        }

        public void Trigger()
        {
            EventData eventData = _eventSelector.SelectEvent(minor: true);
            if (eventData == null) return;
            _eventTrigger.TriggerMinorEvent(eventData, _dayTracker.CurrentDay);
        }
    }
}

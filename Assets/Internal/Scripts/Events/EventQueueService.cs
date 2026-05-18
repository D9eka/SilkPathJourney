using System;
using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Save;

namespace Internal.Scripts.Events
{
    [Serializable]
    public class QueuedEvent
    {
        public string EventId;
        public int TriggerAfterDay;
    }

    public class EventQueueService
    {
        private readonly EventDatabase _eventDatabase;
        private readonly SaveRepository _saveRepository;

        public EventQueueService(EventDatabase eventDatabase, SaveRepository saveRepository)
        {
            _eventDatabase = eventDatabase;
            _saveRepository = saveRepository;
        }

        private List<QueuedEvent> Queue => _saveRepository.Data.EventQueue;

        public void Enqueue(string eventId, int delayDays)
        {
            int currentDay = _saveRepository.Data.Player.CurrentDay;
            Queue.Add(new QueuedEvent { EventId = eventId, TriggerAfterDay = currentDay + delayDays });
        }

        public EventData TryPeek(int currentDay)
        {
            foreach (var queued in Queue)
            {
                if (currentDay >= queued.TriggerAfterDay)
                    return _eventDatabase.GetById(queued.EventId);
            }
            return null;
        }

        public EventData TryDequeue(int currentDay)
        {
            for (int i = 0; i < Queue.Count; i++)
            {
                if (currentDay >= Queue[i].TriggerAfterDay)
                {
                    var eventData = _eventDatabase.GetById(Queue[i].EventId);
                    Queue.RemoveAt(i);
                    return eventData;
                }
            }
            return null;
        }
    }
}

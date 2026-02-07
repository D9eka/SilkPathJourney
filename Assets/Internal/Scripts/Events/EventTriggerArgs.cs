using Internal.Scripts.Events.Data;

namespace Internal.Scripts.Events
{
    public readonly struct EventTriggerArgs
    {
        public EventData EventData { get; }
        public string NearestNodeId { get; }

        public EventTriggerArgs(EventData eventData, string nearestNodeId)
        {
            EventData = eventData;
            NearestNodeId = nearestNodeId;
        }
    }
}

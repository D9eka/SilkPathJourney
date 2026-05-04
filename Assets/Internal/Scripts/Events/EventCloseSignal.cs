using System;
using Internal.Scripts.Events.Data;

namespace Internal.Scripts.Events
{
    public sealed class EventCloseSignal
    {
        public event Action<EventData> Closed;
        public int LastChoiceIndex { get; set; } = -1;
        internal void RaiseClosed(EventData ev) => Closed?.Invoke(ev);
    }
}

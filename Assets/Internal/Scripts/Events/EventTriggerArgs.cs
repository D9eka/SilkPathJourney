using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events.Data;

namespace Internal.Scripts.Events
{
    public readonly struct EventTriggerArgs
    {
        public EventData EventData { get; }
        public CityData City { get; }
        public bool IsAtCity { get; }
        public object[] FormatArgs { get; }

        public EventTriggerArgs(EventData eventData, CityData city, bool isAtCity, object[] formatArgs = null)
        {
            EventData = eventData;
            City = city;
            IsAtCity = isAtCity;
            FormatArgs = formatArgs;
        }
    }
}

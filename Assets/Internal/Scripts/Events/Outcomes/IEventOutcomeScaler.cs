using Internal.Scripts.Events.Data;

namespace Internal.Scripts.Events.Outcomes
{
    public interface IEventOutcomeScaler
    {
        float GetMultiplier(EventData eventData);
    }
}

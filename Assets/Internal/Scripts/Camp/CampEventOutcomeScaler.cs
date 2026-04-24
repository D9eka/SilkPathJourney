using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Events.Outcomes;

namespace Internal.Scripts.Camp
{
    public sealed class CampEventOutcomeScaler : IEventOutcomeScaler
    {
        private readonly CampActionService _campActionService;

        public CampEventOutcomeScaler(CampActionService campActionService)
        {
            _campActionService = campActionService;
        }

        public float GetMultiplier(EventData eventData)
        {
            var action = ParseCampAction(eventData);
            if (!action.HasValue) return 1f;
            return _campActionService.GetCurrentDiminishingMultiplier(action.Value);
        }

        public static CampActionType? ParseCampAction(EventData eventData)
        {
            if (eventData == null) return null;
            return eventData.Category switch
            {
                EventCategory.CampForage => CampActionType.Forage,
                EventCategory.CampRest   => CampActionType.Rest,
                EventCategory.CampRepair => CampActionType.Repair,
                EventCategory.CampOrder  => CampActionType.Order,
                _ => (CampActionType?)null,
            };
        }
    }
}

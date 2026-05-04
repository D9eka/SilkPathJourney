using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;

namespace Internal.Scripts.Quests
{
    public enum QuestCityIndicator
    {
        None,
        NewAvailable,
        ActiveStageHere,
    }

    public class QuestCityIndicatorService
    {
        private readonly QuestAvailabilityService _availability;

        public QuestCityIndicatorService(QuestAvailabilityService availability)
        {
            _availability = availability;
        }

        public QuestCityIndicator GetIndicator(string cityId)
        {
            if (string.IsNullOrEmpty(cityId))
                return QuestCityIndicator.None;

            if (_availability.HasActiveStageInCity(cityId))
                return QuestCityIndicator.ActiveStageHere;

            if (_availability.HasAvailableInCity(cityId))
                return QuestCityIndicator.NewAvailable;

            return QuestCityIndicator.None;
        }

        public string GetIndicatorText(string cityId) => GetIndicator(cityId) switch
        {
            QuestCityIndicator.NewAvailable => LocalizationService.Resolve(
                LocUI.Table, LocUI.UI_QuestIndicator_New),
            QuestCityIndicator.ActiveStageHere => LocalizationService.Resolve(
                LocUI.Table, LocUI.UI_QuestIndicator_Stage),
            _ => null,
        };
    }
}

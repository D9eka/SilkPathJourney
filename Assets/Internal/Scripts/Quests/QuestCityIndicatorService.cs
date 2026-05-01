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
        private readonly QuestRepository _questRepository;

        public QuestCityIndicatorService(QuestRepository questRepository)
        {
            _questRepository = questRepository;
        }

        public QuestCityIndicator GetIndicator(string cityId)
        {
            if (string.IsNullOrEmpty(cityId))
                return QuestCityIndicator.None;

            if (_questRepository.HasActiveStageInCity(cityId))
                return QuestCityIndicator.ActiveStageHere;

            if (_questRepository.HasAvailableInCity(cityId))
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

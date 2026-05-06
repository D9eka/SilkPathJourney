using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using Internal.Scripts.UI.WorldLabel;
using UnityEngine;

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

        public Sprite GetIndicatorIcon(string cityId, QuestIndicatorIcons icons)
        {
            if (icons == null) return null;
            return GetIndicator(cityId) switch
            {
                QuestCityIndicator.NewAvailable => icons.NewAvailable,
                QuestCityIndicator.ActiveStageHere => icons.ActiveStage,
                _ => null,
            };
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

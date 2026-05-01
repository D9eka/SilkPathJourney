using Internal.Scripts.Quests;
using Internal.Scripts.UI.Localization;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.WorldLabel
{
    [CreateAssetMenu(menuName = "SPJ/Quests/Quest Indicator Icons", fileName = "QuestIndicatorIcons")]
    public class QuestIndicatorIcons : ScriptableObject
    {
        [SerializeField] public Sprite NewAvailable;
        [SerializeField] public Sprite ActiveStage;
        [SerializeField] public LocalizedString NewAvailableTitle;
        [SerializeField] public LocalizedString NewAvailableDescription;
        [SerializeField] public LocalizedString ActiveStageTitle;
        [SerializeField] public LocalizedString ActiveStageDescription;

        public string GetTooltipTitle(QuestCityIndicator indicator)
        {
            return indicator switch
            {
                QuestCityIndicator.NewAvailable => LocalizationService.ResolveString(
                    NewAvailableTitle, "Quest Available", "QuestIndicator.NewAvailable.Title"),
                QuestCityIndicator.ActiveStageHere => LocalizationService.ResolveString(
                    ActiveStageTitle, "Quest In Progress", "QuestIndicator.ActiveStage.Title"),
                _ => string.Empty
            };
        }

        public string GetTooltipDescription(QuestCityIndicator indicator)
        {
            return indicator switch
            {
                QuestCityIndicator.NewAvailable => LocalizationService.ResolveString(
                    NewAvailableDescription, string.Empty, "QuestIndicator.NewAvailable.Desc"),
                QuestCityIndicator.ActiveStageHere => LocalizationService.ResolveString(
                    ActiveStageDescription, string.Empty, "QuestIndicator.ActiveStage.Desc"),
                _ => string.Empty
            };
        }
    }
}

using Internal.Scripts.Quests.Data;
using Internal.Scripts.Quests.Generated;
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
        private readonly QuestDatabase _questDatabase;

        public QuestCityIndicatorService(QuestRepository questRepository, QuestDatabase questDatabase)
        {
            _questRepository = questRepository;
            _questDatabase = questDatabase;
        }

        public QuestCityIndicator GetIndicator(string cityId)
        {
            if (string.IsNullOrEmpty(cityId))
                return QuestCityIndicator.None;

            if (HasActiveQuestStageInCity(cityId))
                return QuestCityIndicator.ActiveStageHere;

            if (HasAvailableQuestInCity(cityId))
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

        private bool HasAvailableQuestInCity(string cityId)
        {
            var allQuests = _questDatabase.Quests;
            if (allQuests == null)
                return false;

            foreach (var quest in allQuests)
            {
                if (quest.StartCityId != cityId)
                    continue;

                if (_questRepository.IsActive(quest.Id) || _questRepository.IsCompleted(quest.Id))
                    continue;

                if (quest.OrderInBranch > 1 && !IsPreviousBranchQuestCompleted(quest))
                    continue;

                return true;
            }

            return false;
        }

        private bool IsPreviousBranchQuestCompleted(QuestData quest)
        {
            var allQuests = _questDatabase.Quests;
            if (allQuests == null)
                return false;

            int prevOrder = quest.OrderInBranch - 1;
            foreach (var candidate in allQuests)
            {
                if (candidate.Branch == quest.Branch && candidate.OrderInBranch == prevOrder)
                    return _questRepository.IsCompleted(candidate.Id);
            }

            return false;
        }

        private bool HasActiveQuestStageInCity(string cityId)
        {
            var activeQuests = _questRepository.GetActiveQuests();
            if (activeQuests == null)
                return false;

            foreach (var entry in activeQuests)
            {
                var questData = _questDatabase.GetById(entry.QuestId);
                if (questData?.Stages == null)
                    continue;

                int stageIndex = entry.CurrentStageIndex;
                if (stageIndex < 0 || stageIndex >= questData.Stages.Count)
                    continue;

                var condition = questData.Stages[stageIndex].AutoCompleteCondition;
                if (condition.Type == QuestStageConditionType.InCity && condition.Param == cityId)
                    return true;
            }

            return false;
        }
    }
}

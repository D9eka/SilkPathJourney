using Internal.Scripts.Quests.Data;

namespace Internal.Scripts.UI.Screens.Hud
{
    public enum TrackerChangeType
    {
        None,
        NewQuest,
        StageAdvanced,
        QuestCompleted,
        QuestFailed,
        QuestSwitched
    }

    public struct QuestTrackerState
    {
        public QuestData Quest;
        public int CurrentStageIndex;
        public int PreviousStageIndex;
        public TrackerChangeType ChangeType;
    }
}

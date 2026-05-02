using System;
using System.Collections.Generic;

namespace Internal.Scripts.Quests.Save
{
    [Serializable]
    public class QuestSaveData
    {
        public List<QuestStateEntry> ActiveQuests = new();
        public List<string> CompletedQuestIds = new();
        public List<string> FailedQuestIds = new();
        public List<string> PendingEndingBranchIds = new();
        public string TrackedQuestId;
    }

    [Serializable]
    public class QuestStateEntry
    {
        public string QuestId;
        public int CurrentStageIndex;
        public int StartDay;
        public string StartCityId;
        public List<QuestFlagEntry> Flags = new();
    }

    [Serializable]
    public class QuestFlagEntry
    {
        public string FlagId;
        public int Value;
    }
}

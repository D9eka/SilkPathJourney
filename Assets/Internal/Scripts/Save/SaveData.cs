using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Journal;
using Internal.Scripts.Meta;
using Internal.Scripts.Npc.Save;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.Quests.Save;
using Internal.Scripts.WorldModifiers;

namespace Internal.Scripts.Save
{
    [Serializable]
    public class SaveData
    {
        public int Version = 2;
        public string SelectedBackgroundId;
        public string SelectedCartClassId;
        public EconomySaveData Economy;
        public PlayerSaveData Player;
        public CameraSaveData Camera;
        public RoadSaveData Roads;
        public NpcSaveData Npcs;
        public PlayerSkillState Skills;
        public PlayerLanguageState Languages;
        public QuestSaveData Quests;
        public WorldModifierSaveData WorldModifiers;
        public RunStatsData RunStats;
        public List<JournalEntry> Journal = new();
    }
}

using System;

namespace Internal.Scripts.Save
{
    [Serializable]
    public class SaveMetadata
    {
        public string SlotId;
        public string DisplayName;
        public long LastSavedTimestamp;
        public int CurrentDay;
        public string CurrentNodeId;
        public int Money;
        public int Food;
        public int CartDurability;
        public int Danger;
        public int PartnerCount;
        public bool IsAutoSave;
        public int FoodMax;
        public int CartDurabilityMax;
        public int DangerMax;
    }
}

using System;
using Internal.Scripts.Economy.Save;

namespace Internal.Scripts.Save
{
    [Serializable]
    public class SaveData
    {
        public int Version = 1;
        public EconomySaveData Economy;
        public PlayerSaveData Player;
    }
}

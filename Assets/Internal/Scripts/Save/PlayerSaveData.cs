using System;
using Internal.Scripts.Player;

namespace Internal.Scripts.Save
{
    [Serializable]
    public class PlayerSaveData
    {
        public string CurrentNodeId;
        public string DestinationNodeId;
        public PlayerState State;

        // Система дней
        public int CurrentDay = 1;
        public int LastEventDay = -2;
    }
}

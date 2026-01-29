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
    }
}

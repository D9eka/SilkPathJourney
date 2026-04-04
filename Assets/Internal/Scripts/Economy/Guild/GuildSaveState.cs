using System;

namespace Internal.Scripts.Economy.Guild
{
    [Serializable]
    public class GuildSaveState
    {
        public bool IsMember;
        public int JoinDay;
        public int CreditAmount;
        public int CreditTakenDay;
        public GuildContract ActiveContract;
        public bool HasActiveContract;
    }
}

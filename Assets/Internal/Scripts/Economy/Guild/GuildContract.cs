using System;

namespace Internal.Scripts.Economy.Guild
{
    [Serializable]
    public struct GuildContract
    {
        public string Id;
        public string OriginCityId;
        public string TargetCityId;
        public int RewardMoney;
        public int ExpirationDay;
        public int GeneratedDay;
    }
}

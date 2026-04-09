using System;

namespace Internal.Scripts.Economy.Guild
{
    public enum GuildContractType
    {
        Courier,
        Cargo
    }

    [Serializable]
    public struct GuildContract
    {
        public string Id;
        public string OriginCityId;
        public string TargetCityId;
        public int RewardMoney;
        public int ExpirationDay;
        public int GeneratedDay;
        public GuildContractType ContractType;
        public string CargoItemId;
        public int CargoAmount;
    }
}

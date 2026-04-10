using Internal.Scripts.Economy.Guild;
using Internal.Scripts.UI.Screens.Shared;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Guild
{
    public enum GuildOfferingAction { Join, TakeCredit, RepayCredit, OpenTrade }

    public sealed class GuildViewState
    {
        public bool IsMember;
        public int PlayerMoney;
        public Sprite MoneyIcon;
        public float BaseWeight;
        public float MaxWeight;
        public Sprite WeightIcon;
        public float TariffDiscountPct;
        public int CurrentDebt;

        public OfferingItem[] Offerings;

        public GuildContractEntry[] AvailableContracts;
        public bool HasActiveContract;
        public GuildContractEntry ActiveContract;
    }

    public readonly struct GuildContractEntry
    {
        public readonly string Id;
        public readonly string TargetCityName;
        public readonly int Reward;
        public readonly int DaysToDeliver;
        public readonly GuildContractType ContractType;
        public readonly string CargoItemName;
        public readonly int CargoAmount;
        public readonly float CargoWeightKg;

        public GuildContractEntry(string id, string targetCityName, int reward, int daysToDeliver,
            GuildContractType contractType = GuildContractType.Courier,
            string cargoItemName = null, int cargoAmount = 0, float cargoWeightKg = 0f)
        {
            Id = id;
            TargetCityName = targetCityName;
            Reward = reward;
            DaysToDeliver = daysToDeliver;
            ContractType = contractType;
            CargoItemName = cargoItemName;
            CargoAmount = cargoAmount;
            CargoWeightKg = cargoWeightKg;
        }
    }
}

using UnityEngine;

namespace Internal.Scripts.Economy.Guild
{
    [CreateAssetMenu(menuName = "SPJ/NPC/Guild Settings", fileName = "GuildSettings")]
    public sealed class GuildSettings : ScriptableObject
    {
        [Header("Contracts")]
        [field: SerializeField, Range(0f, 1f)] public float ContractAcceptChance { get; private set; } = 0.9f;
        [field: SerializeField] public float ContractRewardPerDay { get; private set; } = 8f;
        [field: SerializeField] public int ContractBaseReward { get; private set; } = 20;
        [field: SerializeField] public float ContractExpirationMult { get; private set; } = 3f;
        [field: SerializeField] public float ContractFallbackDays { get; private set; } = 10f;

        [Header("Guild Economy")]
        [field: SerializeField] public int GuildRefillThreshold { get; private set; } = 100;
        [field: SerializeField] public int GuildRefillAmount { get; private set; } = 200;
        [field: SerializeField] public int GuildStartingMoney { get; private set; } = 500;

        [Header("Tithe")]
        [field: SerializeField, Range(0f, 0.5f)] public float TitheCraftPct { get; private set; } = 0.10f;
        [field: SerializeField, Range(0f, 0.5f)] public float TitheLuxuryPct { get; private set; } = 0.10f;
        [field: SerializeField, Range(0f, 0.5f)] public float TitheExoticPct { get; private set; } = 0.05f;

        [Header("Tariff")]
        [field: SerializeField, Range(0f, 1f)] public float MemberTariffDiscount { get; private set; } = 0.30f;
        [field: SerializeField, Range(0f, 1f)] public float GuildTariffShare { get; private set; } = 0.30f;

        [Header("Credit")]
        [field: SerializeField] public int PlayerCreditAmount { get; private set; } = 200;
        [field: SerializeField] public int PlayerCreditRepayment { get; private set; } = 220;
        [field: SerializeField] public int JoinCost { get; private set; } = 50;

        [Header("Credit Overdue")]
        [field: SerializeField] public int CreditOverdueDays { get; private set; } = 30;
        [field: SerializeField] public int CreditExpelDays { get; private set; } = 60;
        [field: SerializeField] public int ReputationOverduePenalty { get; private set; } = -5;
    }
}

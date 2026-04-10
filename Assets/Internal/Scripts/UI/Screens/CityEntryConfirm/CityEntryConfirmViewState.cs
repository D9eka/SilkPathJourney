using Internal.Scripts.Economy.Cities.Tariff;
using Internal.Scripts.Economy.Cities.Smuggling;
using Internal.Scripts.UI.Components;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.CityEntryConfirm
{
    public enum CityEntryPhase { Preview, Result }

    public sealed class CityEntryConfirmViewState
    {
        public string CityName;
        public CityEntryPhase Phase;

        public IconLabelEntry CityType;
        public IconLabelEntry[] Buildings;
        public IconLabelEntry[] Modifiers;
        public bool HasQuest;

        public int TariffAmount;
        public bool IsGuildMember;
        public float GuildDiscountPct;
        public int PlayerMoney;
        public int PlayerMoneyBefore;
        public Sprite MoneyIcon;
        public bool CanAfford;
        public bool HasSmugglingRisk;
        public float DetectionChance;
        public int HiddenItemCount;

        public TariffResult TariffResult;
        public SmugglingCheckResult SmugglingResult;
    }
}

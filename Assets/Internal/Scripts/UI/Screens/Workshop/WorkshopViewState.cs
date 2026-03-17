using System.Collections.Generic;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.UI.Screens.Shared;

namespace Internal.Scripts.UI.Screens.Workshop
{
    public sealed class WorkshopViewState
    {
        public int PlayerMoney;
        public string WeightFormatted;
        public string SpeedFormatted;
        public MainCartViewData MainCart;
        public IReadOnlyList<ExtraCartViewData> AvailableExtraCarts;
        public IReadOnlyList<UpgradeViewData> Upgrades;
        public int CurrentExtraCartCount;
        public int MaxExtraCartSlots;
        public string SlotsFormatted;
        public string QuestDescription;
        public string QuestEventId;
        public bool HasQuest;

        public WorkshopViewState(int playerMoney, string weightFormatted, string speedFormatted,
            MainCartViewData mainCart, IReadOnlyList<ExtraCartViewData> availableExtraCarts,
            IReadOnlyList<UpgradeViewData> upgrades,
            int currentExtraCartCount, int maxExtraCartSlots, string slotsFormatted,
            bool hasQuest, string questDescription, string questEventId)
        {
            PlayerMoney = playerMoney;
            WeightFormatted = weightFormatted;
            SpeedFormatted = speedFormatted;
            MainCart = mainCart;
            AvailableExtraCarts = availableExtraCarts;
            Upgrades = upgrades;
            CurrentExtraCartCount = currentExtraCartCount;
            MaxExtraCartSlots = maxExtraCartSlots;
            SlotsFormatted = slotsFormatted;
            HasQuest = hasQuest;
            QuestDescription = questDescription;
            QuestEventId = questEventId;
        }
    }

    public readonly struct UpgradeViewData
    {
        public readonly string Title;
        public readonly string Description;
        public readonly string ButtonText;
        public readonly bool CanBuy;
        public readonly bool AlreadyOwned;
        public readonly CaravanUpgradeType Type;

        public UpgradeViewData(string title, string description, string buttonText,
            bool canBuy, bool alreadyOwned, CaravanUpgradeType type)
        {
            Title = title;
            Description = description;
            ButtonText = buttonText;
            CanBuy = canBuy;
            AlreadyOwned = alreadyOwned;
            Type = type;
        }
    }

    public readonly struct MainCartViewData
    {
        public readonly CartStatsData Stats;
        public readonly string UpgradeEffectText;
        public readonly bool CanUpgrade;
        public readonly bool IsMaxLevel;
        public readonly int UpgradeCost;

        public MainCartViewData(CartStatsData stats, string upgradeEffectText, bool canUpgrade, bool isMaxLevel, int upgradeCost)
        {
            Stats = stats;
            UpgradeEffectText = upgradeEffectText;
            CanUpgrade = canUpgrade;
            IsMaxLevel = isMaxLevel;
            UpgradeCost = upgradeCost;
        }
    }

    public readonly struct ExtraCartViewData
    {
        public readonly CartStatsData Stats;
        public readonly string CountText;
        public readonly int BuyPrice;
        public readonly int SellPrice;
        public readonly bool CanBuy;
        public readonly bool CanSell;
        public readonly ExtraCartType Type;

        public ExtraCartViewData(CartStatsData stats, string countText,
            int buyPrice, int sellPrice, bool canBuy, bool canSell, ExtraCartType type)
        {
            Stats = stats;
            CountText = countText;
            BuyPrice = buyPrice;
            SellPrice = sellPrice;
            CanBuy = canBuy;
            CanSell = canSell;
            Type = type;
        }
    }
}

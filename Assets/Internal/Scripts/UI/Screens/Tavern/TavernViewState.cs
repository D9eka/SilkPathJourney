using System.Collections.Generic;
using Internal.Scripts.Caravan.Generated;

namespace Internal.Scripts.UI.Screens.Tavern
{
    public sealed class TavernViewState
    {
        public readonly int PlayerMoney;
        public readonly string RumorsText;
        public readonly IReadOnlyList<RoadInfoEntry> RoadInfos;
        public readonly IReadOnlyList<PriceTipEntry> PriceTips;
        public readonly IReadOnlyList<CompanionHireData> AvailableCompanions;
        public readonly int CurrentCompanionCount;
        public readonly int MaxCompanions;
        public readonly string SlotsFormatted;
        public readonly bool HasQuest;
        public readonly string QuestDescription;
        public readonly string QuestEventId;

        public TavernViewState(
            int playerMoney,
            string rumorsText,
            IReadOnlyList<RoadInfoEntry> roadInfos,
            IReadOnlyList<PriceTipEntry> priceTips,
            IReadOnlyList<CompanionHireData> availableCompanions,
            int currentCompanionCount,
            int maxCompanions,
            string slotsFormatted,
            bool hasQuest,
            string questDescription,
            string questEventId)
        {
            PlayerMoney = playerMoney;
            RumorsText = rumorsText;
            RoadInfos = roadInfos;
            PriceTips = priceTips;
            AvailableCompanions = availableCompanions;
            CurrentCompanionCount = currentCompanionCount;
            MaxCompanions = maxCompanions;
            SlotsFormatted = slotsFormatted;
            HasQuest = hasQuest;
            QuestDescription = questDescription;
            QuestEventId = questEventId;
        }
    }

    public readonly struct RoadInfoEntry
    {
        public readonly string RouteName;
        public readonly string Description;
        public readonly string ButtonText;
        public readonly bool CanBuy;
        public readonly int Index;

        public RoadInfoEntry(string routeName, string description, string buttonText, bool canBuy, int index)
        {
            RouteName = routeName;
            Description = description;
            ButtonText = buttonText;
            CanBuy = canBuy;
            Index = index;
        }
    }

    public readonly struct PriceTipEntry
    {
        public readonly string CityId;
        public readonly string CityName;
        public readonly string ButtonText;
        public readonly string Description;
        public readonly bool CanBuy;
        public readonly int Index;

        public PriceTipEntry(string cityId, string cityName, string buttonText, string description, bool canBuy, int index)
        {
            CityId = cityId;
            CityName = cityName;
            ButtonText = buttonText;
            Description = description;
            CanBuy = canBuy;
            Index = index;
        }
    }

    public readonly struct CompanionHireData
    {
        public readonly string TypeAndName;
        public readonly string QualityName;
        public readonly string EffectText;
        public readonly string HireCostText;
        public readonly string DailyCostText;
        public readonly bool CanHire;
        public readonly int Index;
        public readonly CompanionType Type;
        public readonly CompanionQuality Quality;

        public CompanionHireData(
            string typeAndName,
            string qualityName,
            string effectText,
            string hireCostText,
            string dailyCostText,
            bool canHire,
            int index,
            CompanionType type,
            CompanionQuality quality)
        {
            TypeAndName = typeAndName;
            QualityName = qualityName;
            EffectText = effectText;
            HireCostText = hireCostText;
            DailyCostText = dailyCostText;
            CanHire = canHire;
            Index = index;
            Type = type;
            Quality = quality;
        }
    }
}

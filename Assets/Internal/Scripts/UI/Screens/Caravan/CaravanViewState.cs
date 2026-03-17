using System.Collections.Generic;

namespace Internal.Scripts.UI.Screens.Caravan
{
    public sealed class CaravanViewState
    {
        public readonly int PlayerMoney;
        public readonly string WeightFormatted;

        public readonly string MoraleFormatted;
        public readonly string DangerFormatted;
        public readonly string FoodFormatted;
        public readonly string SpeedFormatted;
        public readonly string CompanionPayFormatted;
        public readonly string HealthFormatted;

        public readonly IReadOnlyList<CompanionCardData> Companions;
        public readonly int MaxCompanions;

        public readonly AnimalViewData Animal;
        public readonly IReadOnlyList<CartViewData> Carts;

        public CaravanViewState(
            int playerMoney, string weightFormatted,
            string moraleFormatted, string dangerFormatted,
            string foodFormatted, string speedFormatted,
            string companionPayFormatted, string healthFormatted,
            IReadOnlyList<CompanionCardData> companions, int maxCompanions,
            AnimalViewData animal, IReadOnlyList<CartViewData> carts)
        {
            PlayerMoney = playerMoney;
            WeightFormatted = weightFormatted;
            MoraleFormatted = moraleFormatted;
            DangerFormatted = dangerFormatted;
            FoodFormatted = foodFormatted;
            SpeedFormatted = speedFormatted;
            CompanionPayFormatted = companionPayFormatted;
            HealthFormatted = healthFormatted;
            Companions = companions;
            MaxCompanions = maxCompanions;
            Animal = animal;
            Carts = carts;
        }
    }

    public readonly struct CompanionCardData
    {
        public readonly string TypeAndName;
        public readonly string StatusText;
        public readonly string EffectText;
        public readonly string DailyCostText;
        public readonly bool IsInjured;
        public readonly bool CanHeal;
        public readonly int Index;

        public CompanionCardData(
            string typeAndName, string statusText, string effectText,
            string dailyCostText, bool isInjured, bool canHeal, int index)
        {
            TypeAndName = typeAndName;
            StatusText = statusText;
            EffectText = effectText;
            DailyCostText = dailyCostText;
            IsInjured = isInjured;
            CanHeal = canHeal;
            Index = index;
        }
    }

    public readonly struct AnimalViewData
    {
        public readonly string TypeName;
        public readonly string StatusText;
        public readonly string EffectText;
        public readonly bool IsInjured;
        public readonly bool CanHeal;
        public readonly int HealCost;

        public AnimalViewData(
            string typeName, string statusText, string effectText,
            bool isInjured, bool canHeal, int healCost)
        {
            TypeName = typeName;
            StatusText = statusText;
            EffectText = effectText;
            IsInjured = isInjured;
            CanHeal = canHeal;
            HealCost = healCost;
        }
    }

    public readonly struct CartViewData
    {
        public readonly string TypeName;
        public readonly bool IsMainCart;
        public readonly string EffectAndCompanionText;
        public readonly string DurabilityText;
        public readonly string ConsumptionText;
        public readonly bool CanRepair;
        public readonly bool CanDiscard;
        public readonly int Index;

        public CartViewData(
            string typeName, bool isMainCart, string effectAndCompanionText,
            string durabilityText, string consumptionText,
            bool canRepair, bool canDiscard, int index)
        {
            TypeName = typeName;
            IsMainCart = isMainCart;
            EffectAndCompanionText = effectAndCompanionText;
            DurabilityText = durabilityText;
            ConsumptionText = consumptionText;
            CanRepair = canRepair;
            CanDiscard = canDiscard;
            Index = index;
        }
    }
}

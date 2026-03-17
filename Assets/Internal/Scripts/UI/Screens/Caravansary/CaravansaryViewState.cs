using System.Collections.Generic;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.UI.Screens.Shared;

namespace Internal.Scripts.UI.Screens.Caravansary
{
    public sealed class CaravansaryViewState
    {
        public readonly int PlayerMoney;
        public readonly string WeightFormatted;
        public readonly IReadOnlyList<OfferingItem> Offerings;
        public readonly AnimalSwitchData CurrentAnimal;
        public readonly IReadOnlyList<AnimalSwitchData> AvailableAnimals;

        public CaravansaryViewState(
            int playerMoney,
            string weightFormatted,
            IReadOnlyList<OfferingItem> offerings,
            AnimalSwitchData currentAnimal,
            IReadOnlyList<AnimalSwitchData> availableAnimals)
        {
            PlayerMoney = playerMoney;
            WeightFormatted = weightFormatted;
            Offerings = offerings;
            CurrentAnimal = currentAnimal;
            AvailableAnimals = availableAnimals;
        }
    }

    public readonly struct CartRepairEntry
    {
        public readonly string Title;
        public readonly string DurabilityText;
        public readonly string PriceText;
        public readonly string RepairButtonText;
        public readonly string RepairMaxButtonText;
        public readonly bool CanRepair;
        public readonly bool CanRepairMax;
        public readonly bool IsVisible;

        public CartRepairEntry(
            string title,
            string durabilityText,
            string priceText,
            string repairButtonText,
            string repairMaxButtonText,
            bool canRepair,
            bool canRepairMax,
            bool isVisible)
        {
            Title = title;
            DurabilityText = durabilityText;
            PriceText = priceText;
            RepairButtonText = repairButtonText;
            RepairMaxButtonText = repairMaxButtonText;
            CanRepair = canRepair;
            CanRepairMax = canRepairMax;
            IsVisible = isVisible;
        }
    }

    public readonly struct AnimalSwitchData
    {
        public readonly string Name;
        public readonly string EffectText;
        public readonly string PriceText;
        public readonly string ConsumptionText;
        public readonly string IncompatibleBiomes;
        public readonly string NeutralBiomes;
        public readonly string CompatibleBiomes;
        public readonly bool IsCurrent;
        public readonly bool CanSwitch;
        public readonly string ButtonText;
        public readonly int AnimalIndex;
        public readonly DraftAnimalType AnimalType;

        public AnimalSwitchData(
            string name,
            string effectText,
            string priceText,
            string consumptionText,
            string incompatibleBiomes,
            string neutralBiomes,
            string compatibleBiomes,
            bool isCurrent,
            bool canSwitch,
            string buttonText,
            int animalIndex,
            DraftAnimalType animalType)
        {
            Name = name;
            EffectText = effectText;
            PriceText = priceText;
            ConsumptionText = consumptionText;
            IncompatibleBiomes = incompatibleBiomes;
            NeutralBiomes = neutralBiomes;
            CompatibleBiomes = compatibleBiomes;
            IsCurrent = isCurrent;
            CanSwitch = canSwitch;
            ButtonText = buttonText;
            AnimalIndex = animalIndex;
            AnimalType = animalType;
        }
    }
}

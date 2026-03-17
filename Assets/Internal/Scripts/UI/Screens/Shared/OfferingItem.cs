using Internal.Scripts.UI.Screens.Caravansary;

namespace Internal.Scripts.UI.Screens.Shared
{
    public enum OfferingType { Simple, CartRepair }

    public readonly struct OfferingItem
    {
        public readonly OfferingType Type;
        public readonly bool IsVisible;
        public readonly int Index;

        public readonly string Title;
        public readonly string Description;
        public readonly string ButtonText;
        public readonly bool CanAction;

        public readonly CartRepairEntry RepairData;

        public OfferingItem(string title, string description, string buttonText, bool canAction, int index)
        {
            Type = OfferingType.Simple;
            IsVisible = true;
            Index = index;
            Title = title;
            Description = description;
            ButtonText = buttonText;
            CanAction = canAction;
            RepairData = default;
        }

        public OfferingItem(CartRepairEntry repairData, int index)
        {
            Type = OfferingType.CartRepair;
            IsVisible = repairData.IsVisible;
            Index = index;
            Title = null;
            Description = null;
            ButtonText = null;
            CanAction = false;
            RepairData = repairData;
        }

        private OfferingItem(int index)
        {
            Type = default;
            IsVisible = false;
            Index = index;
            Title = null;
            Description = null;
            ButtonText = null;
            CanAction = false;
            RepairData = default;
        }

        public static OfferingItem Hidden(int index) => new(index);
    }
}

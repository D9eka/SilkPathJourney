using Internal.Scripts.Camp;
using Internal.Scripts.UI.Components;

namespace Internal.Scripts.UI.Screens.Camp
{
    public readonly struct CampActionButtonState
    {
        public readonly CampActionType ActionType;
        public readonly string Name;
        public readonly IconLabelEntry Effect;
        public readonly IconLabelEntry Cost;
        public readonly string HintText;
        public readonly bool IsAvailable;

        public CampActionButtonState(
            CampActionType actionType,
            string name,
            IconLabelEntry effect,
            IconLabelEntry cost,
            string hintText,
            bool isAvailable)
        {
            ActionType = actionType;
            Name = name;
            Effect = effect;
            Cost = cost;
            HintText = hintText;
            IsAvailable = isAvailable;
        }
    }
}

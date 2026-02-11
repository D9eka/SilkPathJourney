using Internal.Scripts.Economy.Cities;

namespace Internal.Scripts.Hud
{
    public readonly struct HudViewState
    {
        public readonly HudMode Mode;
        public readonly int ActiveSpeedIndex;
        public readonly CityData City;

        public HudViewState(HudMode mode, int activeSpeedIndex, CityData city)
        {
            Mode = mode;
            ActiveSpeedIndex = activeSpeedIndex;
            City = city;
        }
    }

    public readonly struct ResourceIndicatorState
    {
        public readonly float Value;
        public readonly float MaxValue;
        public readonly int Change;
        public readonly bool Animate;
        public readonly bool IncreaseIsPositive;
        public readonly bool Visible;

        public ResourceIndicatorState(float value, float maxValue, int change, bool animate, bool increaseIsPositive, bool visible = true)
        {
            Value = value;
            MaxValue = maxValue;
            Change = change;
            Animate = animate;
            IncreaseIsPositive = increaseIsPositive;
            Visible = visible;
        }
    }

    public readonly struct HudResourceViewState
    {
        public readonly ResourceIndicatorState Food;
        public readonly ResourceIndicatorState PlayerCart;
        public readonly ResourceIndicatorState OtherCarts;
        public readonly ResourceIndicatorState Danger;

        public HudResourceViewState(
            ResourceIndicatorState food,
            ResourceIndicatorState playerCart,
            ResourceIndicatorState otherCarts,
            ResourceIndicatorState danger)
        {
            Food = food;
            PlayerCart = playerCart;
            OtherCarts = otherCarts;
            Danger = danger;
        }
    }
}

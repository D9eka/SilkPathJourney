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
}

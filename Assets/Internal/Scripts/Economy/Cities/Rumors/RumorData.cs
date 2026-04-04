using Internal.Scripts.Economy.Cities;

namespace Internal.Scripts.Economy.Cities.Rumors
{
    public readonly struct RumorData
    {
        public readonly CityData City;
        public readonly string ModifierId;
        public readonly string ModifierName;
        public readonly int StartDay;
        public readonly int RemainingDays;

        public RumorData(CityData city, string modifierId, string modifierName, int startDay, int remainingDays)
        {
            City = city;
            ModifierId = modifierId;
            ModifierName = modifierName;
            StartDay = startDay;
            RemainingDays = remainingDays;
        }
    }
}

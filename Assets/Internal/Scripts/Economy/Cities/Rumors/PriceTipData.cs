using Internal.Scripts.Economy.Cities;

namespace Internal.Scripts.Economy.Cities.Rumors
{
    public readonly struct PriceTipData
    {
        public readonly CityData City;
        public readonly int DistanceHops;
        public readonly int Cost;

        public PriceTipData(CityData city, int distanceHops, int cost)
        {
            City = city;
            DistanceHops = distanceHops;
            Cost = cost;
        }
    }
}

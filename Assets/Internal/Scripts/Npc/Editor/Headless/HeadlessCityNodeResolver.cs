using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;

namespace Internal.Scripts.Npc.Editor.Headless
{
    public sealed class HeadlessCityNodeResolver : ICityNodeResolver
    {
        private readonly Dictionary<string, CityData> _nodeToCity;

        public HeadlessCityNodeResolver(RoadGraphSnapshot snapshot, EconomyDatabase economyDb)
        {
            _nodeToCity = new Dictionary<string, CityData>(snapshot.CityNodes.Count);
            foreach (var entry in snapshot.CityNodes)
            {
                CityData city = economyDb.Cities.Find(c => c.Id == entry.CityId);
                if (city != null)
                    _nodeToCity[entry.NodeId] = city;
            }
        }

        public bool TryGetCityByNodeId(string nodeId, out CityData city) =>
            _nodeToCity.TryGetValue(nodeId, out city);
    }
}

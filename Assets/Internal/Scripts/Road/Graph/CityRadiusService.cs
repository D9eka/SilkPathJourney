using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Utils;

namespace Internal.Scripts.Road.Graph
{
    public sealed class CityRadiusService
    {
        private readonly IRoadNetwork _roadNetwork;
        private readonly ICityNodeResolver _cityNodeResolver;

        public CityRadiusService(IRoadNetwork roadNetwork, ICityNodeResolver cityNodeResolver)
        {
            _roadNetwork = roadNetwork;
            _cityNodeResolver = cityNodeResolver;
        }

        public List<CityData> GetCitiesInRadius(string fromNodeId, int maxHops)
        {
            var nodeIds = BfsUtility.NodesInRadius(
                fromNodeId,
                maxHops,
                nodeId => _roadNetwork.GetOutgoingEdges(nodeId).Select(e => e.ToNodeId));

            var result = new List<CityData>(nodeIds.Count);
            foreach (string nodeId in nodeIds)
            {
                if (_cityNodeResolver.TryGetCityByNodeId(nodeId, out CityData city))
                    result.Add(city);
            }
            return result;
        }
    }
}

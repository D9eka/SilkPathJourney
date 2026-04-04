using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;

namespace Internal.Scripts.Npc.Routing
{
    public interface INpcRouteDecisionEnvironment
    {
        IReadOnlyList<string> CityNodeIds { get; }
        bool TryGetCityByNodeId(string nodeId, out CityData city);
        float EstimateTravelDays(string fromNodeId, string toNodeId, float speedMetersPerDay);
        string FindNearestCityNode(string currentNodeId);
    }
}

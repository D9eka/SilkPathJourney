using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Road.Nodes;

namespace Internal.Scripts.Economy.Cities
{
    public interface ICityNodeResolver
    {
        bool TryGetCityByNodeId(string nodeId, out CityData city);
    }

    public static class CityNodeResolverExtensions
    {
        public static Biome ResolveRoadBiome(this ICityNodeResolver resolver, string startNodeId, string endNodeId)
        {
            if (resolver.TryGetCityByNodeId(startNodeId, out var city) && city.Biome != Biome.Unknown)
                return city.Biome;
            if (resolver.TryGetCityByNodeId(endNodeId, out city) && city.Biome != Biome.Unknown)
                return city.Biome;
            return Biome.Plains;
        }
    }

    public sealed class CityNodeResolver : ICityNodeResolver
    {
        private readonly IRoadNodeLookup _nodeLookup;

        public CityNodeResolver(IRoadNodeLookup nodeLookup)
        {
            _nodeLookup = nodeLookup;
        }

        public bool TryGetCityByNodeId(string nodeId, out CityData city)
        {
            city = null;
            if (string.IsNullOrWhiteSpace(nodeId))
                return false;

            if (!_nodeLookup.TryGetTransform(nodeId, out var transform) || transform == null)
                return false;

            CityNodeLink link = transform.GetComponent<CityNodeLink>();
            if (link == null || link.City == null)
                return false;

            city = link.City;
            return true;
        }
    }
}

using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Utils;

namespace Internal.Scripts.Economy.Cities
{
    public sealed class CultureDistanceService
    {
        private readonly Dictionary<CultureId, HashSet<CultureId>> _adjacency;
        private readonly Dictionary<(CultureId, CultureId), int> _cache = new();

        public CultureDistanceService(CultureAdjacencyData adjacencyData)
        {
            _adjacency = BuildAdjacencyFromData(adjacencyData);
        }

        public int GetDistance(CultureId a, CultureId b)
        {
            if (a == b) return 0;
            if (a == CultureId.None || b == CultureId.None) return 0;

            var key = a < b ? (a, b) : (b, a);
            if (_cache.TryGetValue(key, out int cached))
                return cached;

            int result = BfsUtility.ShortestDistance(a, b,
                c => _adjacency.TryGetValue(c, out var n) ? n : System.Array.Empty<CultureId>());
            _cache[key] = result;
            return result;
        }

        public float GetExoticityMultiplier(CultureId origin, CultureId target, float perStepBonus)
        {
            return 1f + GetDistance(origin, target) * perStepBonus;
        }

        private static Dictionary<CultureId, HashSet<CultureId>> BuildAdjacencyFromData(CultureAdjacencyData data)
        {
            var result = new Dictionary<CultureId, HashSet<CultureId>>();
            if (data == null) return result;

            foreach (var pair in data.Adjacencies)
            {
                if (!result.TryGetValue(pair.A, out var setA))
                {
                    setA = new HashSet<CultureId>();
                    result[pair.A] = setA;
                }
                setA.Add(pair.B);

                if (!result.TryGetValue(pair.B, out var setB))
                {
                    setB = new HashSet<CultureId>();
                    result[pair.B] = setB;
                }
                setB.Add(pair.A);
            }

            return result;
        }
    }
}

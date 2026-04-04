using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Npc.Routing;
using Internal.Scripts.Road.Graph;
using UnityEngine;

namespace Internal.Scripts.Npc.Editor.Headless
{
    internal sealed class HeadlessRouteDecisionEnvironment : INpcRouteDecisionEnvironment
    {
        private readonly IReadOnlyList<string> _cityNodeIds;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly RoadGraphSnapshot _snapshot;
        private readonly System.Random _rng;

        public HeadlessRouteDecisionEnvironment(
            IReadOnlyList<string> cityNodeIds,
            ICityNodeResolver cityNodeResolver,
            RoadGraphSnapshot snapshot,
            System.Random rng)
        {
            _cityNodeIds = cityNodeIds;
            _cityNodeResolver = cityNodeResolver;
            _snapshot = snapshot;
            _rng = rng;
        }

        public IReadOnlyList<string> CityNodeIds => _cityNodeIds;

        public bool TryGetCityByNodeId(string nodeId, out CityData city)
        {
            return _cityNodeResolver.TryGetCityByNodeId(nodeId, out city);
        }

        public float EstimateTravelDays(string fromNodeId, string toNodeId, float speedMetersPerDay)
        {
            if (speedMetersPerDay <= 0f)
                return 1f;

            float dist = _snapshot.GetDistance(fromNodeId, toNodeId);
            if (dist >= float.MaxValue)
                return 1f;

            return Mathf.Max(1f, dist / speedMetersPerDay);
        }

        public string FindNearestCityNode(string currentNodeId)
        {
            var candidates = new List<string>(_cityNodeIds);
            candidates.Remove(currentNodeId);

            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            float minDist = float.MaxValue;
            string nearest = null;
            foreach (string nodeId in candidates)
            {
                float dist = _snapshot.GetDistance(currentNodeId, nodeId);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = nodeId;
                }
            }

            return nearest ?? (candidates.Count > 0 ? candidates[0] : null);
        }
    }
}

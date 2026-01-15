using System.Collections.Generic;
using Internal.Scripts.Road.Core;

namespace Internal.Scripts.Road.Path
{
    public sealed class RoadSamplerCache
    {
        private readonly Dictionary<RoadRuntime, RoadPolylineSampler> _samplers = new();

        public bool TryGetSampler(RoadRuntime runtime, out RoadPolylineSampler sampler)
        {
            sampler = null;

            if (runtime == null || runtime.Data == null)
                return false;

            if (runtime.Data.PointsLocal == null || runtime.Data.PointsLocal.Count < 2)
                return false;

            if (_samplers.TryGetValue(runtime, out sampler))
                return true;

            sampler = new RoadPolylineSampler(runtime.Data.PointsLocal);
            _samplers[runtime] = sampler;
            return true;
        }
    }
}

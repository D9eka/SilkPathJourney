using Internal.Scripts.Road.Core;
using Internal.Scripts.World.Roads;
using UnityEngine;

namespace Internal.Scripts.Road.Path
{
    public sealed class RoadPoseSampler
    {
        public RoadPose Sample(
            RoadPolylineSampler sampler,
            float distanceAlongPolyline,
            Transform worldRoot,
            Transform runtimeTransformFallback,
            RoadData roadData,
            RoadLane lane,
            float lateralOffsetMeters,
            bool isForward = true)
        {
            Vector3 pLocal = sampler.GetPositionLocal(distanceAlongPolyline);
            Vector3 tLocal = sampler.GetTangentLocal(distanceAlongPolyline);

            if (!isForward) tLocal = -tLocal;

            Vector3 rightLocal = sampler.GetRightLocal(distanceAlongPolyline);
            if (!isForward) rightLocal = -rightLocal;

            float laneOffset = RoadLaneUtility.ComputeLaneOffsetMeters(roadData, lane, lateralOffsetMeters);
            pLocal += rightLocal * laneOffset;

            Vector3 worldPos = worldRoot != null
                ? worldRoot.TransformPoint(pLocal)
                : runtimeTransformFallback.TransformPoint(pLocal);

            Vector3 worldFwd = worldRoot != null
                ? worldRoot.TransformDirection(tLocal)
                : runtimeTransformFallback.TransformDirection(tLocal);

            worldFwd = worldFwd.sqrMagnitude > 1e-6f ? worldFwd.normalized : Vector3.forward;
            return new RoadPose(worldPos, worldFwd);
        }
    }
}
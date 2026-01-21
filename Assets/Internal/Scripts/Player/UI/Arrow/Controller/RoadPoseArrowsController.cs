using System.Collections.Generic;
using Internal.Scripts.Player.Path;
using Internal.Scripts.Player.UI.Arrow.DirectionCalculation;
using Internal.Scripts.Player.UI.Arrow.JunctionBalancer;
using Internal.Scripts.Player.UI.Arrow.Placement;
using Internal.Scripts.Player.UI.Arrow.PositionCalculation;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.Player.UI.Arrow.Controller
{
    public sealed class RoadPoseArrowsController : IArrowsController
    {
        private readonly IArrowPositionCalculator _positionCalculator;
        private readonly IArrowDirectionCalculator _directionCalculator;
        private readonly IArrowPlacementService _placementService;
        private readonly IArrowJunctionBalancer _balancer;

        public RoadPoseArrowsController(
            IArrowPositionCalculator positionCalculator,
            IArrowDirectionCalculator directionCalculator,
            IArrowPlacementService placementService,
            IArrowJunctionBalancer balancer)
        {
            _positionCalculator = positionCalculator;
            _directionCalculator = directionCalculator;
            _placementService = placementService;
            _balancer = balancer;
        }

        public List<ArrowView> GetAllArrows()
        {
            return _placementService.GetAllArrows();
        }

        public void CreateArrows(List<RoadPathSegment> allOptions, PathHints pathHints)
        {
            if (allOptions.Count == 0)
                return;

            List<ArrowData> arrowDataList = new List<ArrowData>();

            foreach (RoadPathSegment segment in allOptions)
            {
                (PathGroup group, float angle) = _balancer.GetPathClassification(segment);
                Vector3 basePos = _positionCalculator.CalculateWorldPosition(segment, RoadLane.Center);
                Vector3 worldPos = _balancer.GetBalancedPosition(basePos, group);
                worldPos = _positionCalculator.SnapToGround(worldPos);
                Vector3 worldDir = _directionCalculator.CalculateWorldDirection(segment, 0f);
                ArrowData arrowData = new ArrowData
                {
                    Segment = segment,
                    WorldPos = worldPos,
                    WorldDir = worldDir,
                    Type = GetArrowType(segment, pathHints)
                };
                arrowDataList.Add(arrowData);
                Debug.Log($"Arrow: Segment={segment.SegmentId}, Group={group}, Angle={angle:F1}°");
            }

            _placementService.PlaceArrows(arrowDataList);
        }

        public void HideArrows()
        {
            _placementService.HideArrows();
        }

        private ArrowType GetArrowType(RoadPathSegment segment, PathHints pathHints)
        {
            if (pathHints == null)
                return ArrowType.Bad;
            if (pathHints.FastestSegment != null &&
                pathHints.FastestSegment.SegmentId == segment.SegmentId)
            {
                return ArrowType.Fastest;
            }
            foreach (RoadPathSegment s in pathHints.LeadingToTargetSegments)
            {
                if (s.SegmentId == segment.SegmentId)
                {
                    return ArrowType.Good;
                }
            }
            return ArrowType.Bad;
        }
    }
}

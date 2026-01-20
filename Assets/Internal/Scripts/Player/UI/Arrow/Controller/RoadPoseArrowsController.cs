using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Player.Path;
using Internal.Scripts.Player.UI.Arrow.DirectionCalculation;
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

        public RoadPoseArrowsController(
            IArrowPositionCalculator positionCalculator,
            IArrowDirectionCalculator directionCalculator,
            IArrowPlacementService placementService)
        {
            _positionCalculator = positionCalculator;
            _directionCalculator = directionCalculator;
            _placementService = placementService;
        }

        public List<ArrowView> GetAllArrows() => _placementService.GetAllArrows();

        public void CreateArrows(IEnumerable<RoadPathSegment> allOptions, PathHints pathHints)
        {
            List<RoadPathSegment> options = allOptions.ToList();
            if (options.Count == 0)
                return;

            List<ArrowData> arrowDataList = options
                .Select(segment => CreateArrowData(segment, pathHints))
                .Where(data => data != null)
                .ToList();

            _placementService.PlaceArrows(arrowDataList);
        }

        public void HideArrows() => _placementService.HideArrows();

        private ArrowData CreateArrowData(RoadPathSegment segment, PathHints pathHints)
        {
            Vector3 worldPos = _positionCalculator.CalculateWorldPosition(segment, 0f,
                RoadLane.Center);

            Vector3 worldDir = _directionCalculator.CalculateWorldDirection(segment, 0f);

            return new ArrowData
            {
                Segment = segment,
                WorldPos = worldPos,
                WorldDir = worldDir,
                Type = GetArrowType(segment, pathHints)
            };
        }

        private ArrowType GetArrowType(RoadPathSegment seg, PathHints hints)
        {
            if (hints == null)
            {
                Debug.LogWarning($"[RoadPoseArrowsController] PathHints is null for segment {seg.SegmentId}");
                return ArrowType.Bad;
            }

            if (hints.FastestSegment != null && hints.FastestSegment.SegmentId.RoadId == seg.SegmentId.RoadId)
            {
                Debug.Log($"[Arrow] Type: Fastest for segment {seg.SegmentId}");
                return ArrowType.Fastest;
            }

            if (hints.LeadingToTargetSegments != null && 
                hints.LeadingToTargetSegments.Any(s => s.SegmentId.RoadId == seg.SegmentId.RoadId))
            {
                Debug.Log($"[Arrow] Type: Good for segment {seg.SegmentId}");
                return ArrowType.Good;
            }

            Debug.Log($"[Arrow] Type: Bad for segment {seg.SegmentId}");
            return ArrowType.Bad;
        }
    }
}
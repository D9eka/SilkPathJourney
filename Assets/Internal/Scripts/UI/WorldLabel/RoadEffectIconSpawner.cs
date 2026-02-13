using System.Collections.Generic;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Positioning;
using Internal.Scripts.UI.Tooltip;
using Internal.Scripts.World.State;
using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel
{
    public class RoadEffectIconSpawner : WorldLabelSpawnerBase
    {
        private const float ICON_HEIGHT = 1.5f;

        private readonly RoadRuntime[] _roads;
        private readonly IRoadSidePositionCalculator _roadSidePositionCalculator;

        public RoadEffectIconSpawner(
            RoadRuntime[] roads,
            WorldStateController worldStateController,
            WorldCanvas worldCanvas,
            IRoadSidePositionCalculator roadSidePositionCalculator)
            : base(worldStateController, worldCanvas)
        {
            _roads = roads;
            _roadSidePositionCalculator = roadSidePositionCalculator;
        }

        protected override bool ShouldShowInViewMode(WorldViewMode viewMode) => true;

        protected override void SpawnLabels()
        {
            foreach (RoadRuntime road in _roads)
            {
                if (road == null || road.Data == null) continue;

                List<Vector3> points = road.Data.PointsLocal;
                if (points == null || points.Count == 0) continue;

                int midIndex = points.Count / 2;
                Vector3 localMid = points[midIndex];
                Vector3 worldMid = road.WorldRoot != null
                    ? road.WorldRoot.TransformPoint(localMid)
                    : localMid;

                Vector3 groundSnappedPos = _roadSidePositionCalculator.SnapToGround(worldMid, ICON_HEIGHT);

                WorldLabelView label = CreateAndConfigureLabel(
                    groundSnappedPos,
                    $"RoadIcon_{road.Data.RoadId}");

                var simpleData = new SimpleTooltipData("Road Effect", "Modifier details coming soon");
                label.SetTooltipProvider(simpleData);
            }
        }
    }
}

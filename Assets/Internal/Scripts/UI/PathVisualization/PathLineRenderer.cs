using System.Collections.Generic;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using Internal.Scripts.UI.Arrow.PositionCalculation;
using UnityEngine;

namespace Internal.Scripts.UI.PathVisualization
{
    public class PathLineRenderer
    {
        private const float HEIGHT_OFFSET = 2f;

        private readonly RoadNetwork _roadNetwork;
        private readonly GroundSnapper _groundSnapper;

        public PathLineRenderer(RoadNetwork roadNetwork, GroundSnapper groundSnapper)
        {
            _roadNetwork = roadNetwork;
            _groundSnapper = groundSnapper;
        }

        public Vector3[] RenderPath(RoadPath path)
        {
            if (path == null || !path.IsValid)
                return System.Array.Empty<Vector3>();

            List<Vector3> allPositions = new List<Vector3>(1000);

            foreach (RoadPathSegment segment in path.Segments)
            {
                if (!_roadNetwork.TryGetSegment(segment.SegmentId, out RoadSegmentData segmentData))
                    continue;

                var points = segmentData.Data.PointsLocal;
                if (points == null || points.Count < 2) continue;

                int count = points.Count;
                for (int i = 0; i < count; i++)
                {
                    int idx = segment.IsForward ? i : count - 1 - i;
                    Vector3 worldPos = segmentData.Runtime.LocalToWorld(points[idx]);
                    allPositions.Add(_groundSnapper.SnapToGround(worldPos, HEIGHT_OFFSET));
                }
            }

            return allPositions.ToArray();
        }
    }
}

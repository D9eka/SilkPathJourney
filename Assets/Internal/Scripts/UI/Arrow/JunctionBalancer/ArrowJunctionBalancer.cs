using System.Collections.Generic;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using Internal.Scripts.UI.Arrow.DirectionCalculation;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.UI.Arrow.JunctionBalancer
{
    public sealed class ArrowJunctionBalancer : IArrowJunctionBalancer, IFixedTickable
    {
        private readonly IArrowDirectionCalculator _directionCalculator;
        private readonly IRoadNetwork _roadNetwork;

        private const float HEIGHT_UP_PCT = 0.35f;
        private const float HEIGHT_DOWN_PCT = 0.35f;
        private const float LATERAL_SHIFT_PCT = 0.35f;
        private const float BACKWARD_ANGLE_THRESHOLD = 140f;
        private readonly HashSet<RoadSegmentId> _badSegments = new HashSet<RoadSegmentId>();

        private RoadAgent _playerRoadAgent;
        private Vector3 _lastPlayerDirection = Vector3.forward;

        public ArrowJunctionBalancer(IArrowDirectionCalculator directionCalculator, IRoadNetwork roadNetwork)
        {
            _directionCalculator = directionCalculator;
            _roadNetwork = roadNetwork;
        }

        public void Initialize(RoadAgent playerRoadAgent)
        {
            _playerRoadAgent = playerRoadAgent;
        }
        
        public void FixedTick()
        {
            if (_playerRoadAgent == null) return;
            UpdatePlayerState(_playerRoadAgent.CurrentPose.Forward);
        }
        
        public Vector3 GetBalancedPosition(Vector3 basePos, PathGroup group, RoadPathSegment segment)
        {
            Vector3 currentForward = _lastPlayerDirection;
            Vector3 currentLeft = Vector3.Cross(Vector3.up, currentForward).normalized;
            Vector3 currentRight = -currentLeft;

            if (!TryGetRoadWidth(segment, out float roadWidth))
            {
                return basePos;
            }

            float lateralShift = roadWidth * LATERAL_SHIFT_PCT;
            float heightUp = roadWidth * HEIGHT_UP_PCT;
            float heightDown = roadWidth * HEIGHT_DOWN_PCT;

            Vector3 horizontalOffset;
            Vector3 verticalOffset;

            switch (group)
            {
                case PathGroup.Left:
                    horizontalOffset = currentRight * lateralShift;
                    verticalOffset = -Vector3.up * heightDown;
                    break;

                case PathGroup.Right:
                    horizontalOffset = currentLeft * lateralShift;
                    verticalOffset = Vector3.up * heightUp;
                    break;

                case PathGroup.Backward:
                default:
                    return basePos;
            }

            return basePos + horizontalOffset + verticalOffset;
        }
        
        public (PathGroup group, float angle) GetPathClassification(RoadPathSegment segment)
        {
            float angle = GetPathAngle(segment);
            PathGroup group = ClassifyPath(angle);
            return (group, angle);
        }

        private float GetPathAngle(RoadPathSegment segment)
        {
            Vector3 pathDirection = _directionCalculator.CalculateWorldDirection(segment, 0f);
            Vector3 currentForward = _lastPlayerDirection;
            Vector3 currentLeft = Vector3.Cross(Vector3.up, currentForward).normalized;

            return GetPathAngleInternal(pathDirection, currentForward, currentLeft);
        }

        private PathGroup ClassifyPath(float angle)
        {
            if (angle > BACKWARD_ANGLE_THRESHOLD || angle < -BACKWARD_ANGLE_THRESHOLD)
            {
                return PathGroup.Backward;
            }

            if (angle > 0f)
            {
                return PathGroup.Left;
            }

            return PathGroup.Right;
        }

        private void UpdatePlayerState(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0.001f)
            {
                _lastPlayerDirection = direction.normalized;
            }
        }
        
        private float GetPathAngleInternal(
            Vector3 pathDirection,
            Vector3 currentForward,
            Vector3 currentLeft)
        {
            pathDirection = new Vector3(pathDirection.x, 0, pathDirection.z).normalized;
            currentForward = new Vector3(currentForward.x, 0, currentForward.z).normalized;

            if (pathDirection.sqrMagnitude < 0.001f || currentForward.sqrMagnitude < 0.001f)
                return 0f;

            float dotLeft = Vector3.Dot(pathDirection, currentLeft);
            float dotForward = Vector3.Dot(pathDirection, currentForward);

            float angle = Mathf.Atan2(dotLeft, dotForward) * Mathf.Rad2Deg;

            return angle;
        }

        private bool TryGetRoadWidth(RoadPathSegment segment, out float roadWidth)
        {
            roadWidth = 0f;
            if (_roadNetwork == null || segment == null)
            {
                return false;
            }

            if (!_roadNetwork.TryGetSegment(segment.SegmentId, out RoadSegmentData data) || data.Data == null)
            {
                LogBadSegment(segment, "RoadData отсутствует");
                return false;
            }

            float width = data.Data.LaneWidth * data.Data.LaneCount;
            if (width <= 0f)
            {
                LogBadSegment(segment, "LaneWidth или LaneCount <= 0");
                return false;
            }

            roadWidth = width;
            return true;
        }

        private void LogBadSegment(RoadPathSegment segment, string reason)
        {
            if (segment == null)
            {
                Debug.LogError("ArrowJunctionBalancer: сегмент отсутствует.");
                return;
            }

            if (_badSegments.Add(segment.SegmentId))
            {
                Debug.LogError($"ArrowJunctionBalancer: не удалось получить ширину дороги для {segment.SegmentId}. Причина: {reason}.");
            }
        }
    }
}

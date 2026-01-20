using Internal.Scripts.Npc.Core;
using Internal.Scripts.Player.UI.Arrow.DirectionCalculation;
using Internal.Scripts.Road.Path;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Player.UI.Arrow.JunctionBalancer
{
    public sealed class ArrowJunctionBalancer : IArrowJunctionBalancer, IFixedTickable
    {
        private readonly IArrowDirectionCalculator _directionCalculator;

        private const float HEIGHT_UP = 5f;
        private const float HEIGHT_DOWN = 2.5f;
        private const float LATERAL_SHIFT = 2.5f;
        private const float BACKWARD_ANGLE_THRESHOLD = 140f;

        private RoadAgent _playerRoadAgent;
        private Vector3 _lastPlayerDirection = Vector3.forward;

        public ArrowJunctionBalancer(IArrowDirectionCalculator directionCalculator)
        {
            _directionCalculator = directionCalculator;
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
        
        public Vector3 GetBalancedPosition(Vector3 basePos, PathGroup group)
        {
            Vector3 currentForward = _lastPlayerDirection;
            Vector3 currentLeft = Vector3.Cross(Vector3.up, currentForward).normalized;
            Vector3 currentRight = -currentLeft;

            Vector3 horizontalOffset;
            Vector3 verticalOffset;

            switch (group)
            {
                case PathGroup.Left:
                    horizontalOffset = currentRight * LATERAL_SHIFT;
                    verticalOffset = -Vector3.up * HEIGHT_DOWN;
                    break;

                case PathGroup.Right:
                    horizontalOffset = currentLeft * LATERAL_SHIFT;
                    verticalOffset = Vector3.up * HEIGHT_UP;
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
    }
}

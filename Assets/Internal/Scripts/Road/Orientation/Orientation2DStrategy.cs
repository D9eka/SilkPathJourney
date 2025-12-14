using UnityEngine;

namespace Internal.Scripts.Road.Orientation
{
    public sealed class Orientation2DStrategy : IOrientationStrategy
    {
        private const float MIN_VECTOR_DEVIATION = 0.0001f;
        
        private float _rotationSpeed;

        public Orientation2DStrategy(float rotationSpeed = 1f)
        {
            _rotationSpeed = rotationSpeed;
        }

        public void SetRotationSpeed(float rotationSpeed)
        {
            _rotationSpeed = rotationSpeed;
        }

        public void Apply(Transform target, Vector3 forward, float deltaTime)
        {
            Vector2 dir = new Vector2(forward.x, forward.y);
            if (dir.sqrMagnitude < MIN_VECTOR_DEVIATION)
                return;

            dir.Normalize();

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);

            target.rotation = Quaternion.Slerp(
                target.rotation,
                targetRot,
                deltaTime * _rotationSpeed
            );
        }
    }
}
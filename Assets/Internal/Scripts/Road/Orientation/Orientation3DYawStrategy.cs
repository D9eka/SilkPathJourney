using UnityEngine;

namespace Internal.Scripts.Road.Orientation
{
    public sealed class Orientation3DYawStrategy : IOrientationStrategy
    {
        private const float MIN_VECTOR_DEVIATION = 0.0001f;
        
        private readonly float _rotationSpeed;

        public Orientation3DYawStrategy(float rotationSpeed)
        {
            _rotationSpeed = rotationSpeed;
        }

        public void Apply(Transform target, Vector3 forward, float deltaTime)
        {
            if (forward.sqrMagnitude < MIN_VECTOR_DEVIATION)
                return;

            forward.y = 0f;
            if (forward.sqrMagnitude < MIN_VECTOR_DEVIATION)
                return;

            forward.Normalize();

            Quaternion targetRot = Quaternion.LookRotation(forward, Vector3.up);
            target.rotation = Quaternion.Slerp(
                target.rotation,
                targetRot,
                deltaTime * _rotationSpeed
            );
        }
    }
}
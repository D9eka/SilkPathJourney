using UnityEngine;

namespace Internal.Scripts.Road.Orientation
{
    public interface IOrientationStrategy
    {
        public void SetRotationSpeed(float rotationSpeed);
        void Apply(Transform target, Vector3 forward, float deltaTime);
    }
}
using UnityEngine;

namespace Internal.Scripts.Road.Orientation
{
    public interface IOrientationStrategy
    {
        void Apply(Transform target, Vector3 forward, float deltaTime);
    }
}
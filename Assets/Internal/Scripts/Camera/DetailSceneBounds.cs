using UnityEngine;

namespace Internal.Scripts.Camera
{
    public class DetailSceneBounds : MonoBehaviour
    {
        [SerializeField] private Transform _center;
        [SerializeField] private Collider _boundsCollider;

        public Collider BoundsCollider => _boundsCollider;
        public Transform CenterTransform => _center;

        public Vector2 Center => new Vector2(_center.position.x, _center.position.z);
        public Vector2 Size => new Vector2(
            _boundsCollider.bounds.size.x, _boundsCollider.bounds.size.z);
    }
}

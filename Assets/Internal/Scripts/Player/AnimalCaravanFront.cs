using UnityEngine;

namespace Internal.Scripts.Player
{
    public sealed class AnimalCaravanFront : MonoBehaviour
    {
        [SerializeField] private Transform _front;

        public Vector3 FrontWorld => _front != null ? _front.position : transform.position;
    }
}

using UnityEngine;

namespace Internal.Scripts.Player
{
    public sealed class CartFollowerView : MonoBehaviour
    {
        [SerializeField] private Transform _riderSlot;
        [SerializeField] private Transform _animalSlot;
        [SerializeField] private Animator _animator;

        private static readonly int IsMoving = Animator.StringToHash("IsMoving");

        private Vector3 _previousPosition;
        private GameObject _rider;
        private GameObject _animal;

        private void Start()
        {
            _previousPosition = transform.position;
        }

        public void UpdateMovementState()
        {
            if (_animator == null)
                return;

            bool moving = (transform.position - _previousPosition).sqrMagnitude > 0.0001f;
            _animator.SetBool(IsMoving, moving);
            _previousPosition = transform.position;
        }

        public void SetRider(GameObject riderPrefab)
        {
            if (_rider != null)
                return;

            Transform parent = _riderSlot != null ? _riderSlot : transform;
            _rider = Instantiate(riderPrefab, parent);
            _rider.transform.localPosition = Vector3.zero;
            _rider.transform.localRotation = Quaternion.identity;
        }

        public void ClearRider()
        {
            if (_rider == null)
                return;

            Destroy(_rider);
            _rider = null;
        }

        public void SetAnimal(GameObject prefab, float scale, Vector3 offset)
        {
            ClearAnimal();
            Transform parent = _animalSlot != null ? _animalSlot : transform;
            _animal = Instantiate(prefab, parent);
            _animal.transform.localPosition = offset;
            _animal.transform.localRotation = Quaternion.identity;
            _animal.transform.localScale = Vector3.one * scale;
        }

        public void ClearAnimal()
        {
            if (_animal == null) return;
            Destroy(_animal);
            _animal = null;
        }

        private void OnDestroy()
        {
            ClearRider();
            ClearAnimal();
        }
    }
}

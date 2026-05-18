using UnityEngine;

namespace Internal.Scripts.Player
{
    public sealed class CartFollowerView : MonoBehaviour
    {
        [SerializeField] private Transform _animalSlot;
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _caravanBack;

        private static readonly int IsMoving = Animator.StringToHash("IsMoving");

        private Vector3 _previousPosition;
        private GameObject _animal;
        private AnimalCaravanFront _animalFront;

        public Vector3 CaravanBackWorld => _caravanBack != null ? _caravanBack.position : transform.position;
        public Vector3 AnimalFrontWorld => _animalFront != null ? _animalFront.FrontWorld : (_animalSlot != null ? _animalSlot.position : transform.position);

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

        public void SetAnimal(GameObject prefab, float scale, Vector3 offset)
        {
            ClearAnimal();
            Transform parent = _animalSlot != null ? _animalSlot : transform;
            _animal = Instantiate(prefab, parent);
            _animal.transform.localPosition = offset;
            _animal.transform.localRotation = Quaternion.identity;
            _animal.transform.localScale = Vector3.one * scale;
            _animalFront = _animal.GetComponent<AnimalCaravanFront>();
        }

        public void ClearAnimal()
        {
            if (_animal == null) return;
            Destroy(_animal);
            _animal = null;
            _animalFront = null;
        }

        private void OnDestroy()
        {
            ClearAnimal();
        }
    }
}

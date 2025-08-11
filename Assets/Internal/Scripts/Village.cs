using Internal.Scripts.InteractableObjects;
using UnityEngine;

namespace Internal.Scripts
{
    public class Village : MonoBehaviour
    {
        [SerializeField] private Vector2 _cameraPosition;
        [Space]
        [SerializeField] private VillageInteractableObject _villageInteractableObject;
        [SerializeField] private SelectVillageInteractableObject[] _selectVillageInteractableObjects;
        [SerializeField] private BuildingInteractableObject[] _buildings;

        public Vector2 CameraPosition => _cameraPosition;
        public VillageInteractableObject VillageInteractableObject => _villageInteractableObject;
        public SelectVillageInteractableObject[] SelectVillageInteractableObjects => _selectVillageInteractableObjects;

        private void Awake()
        {
            _villageInteractableObject.Initialize(this);
            _villageInteractableObject.SwitchObjectState(false);

            foreach (SelectVillageInteractableObject selectVillageInteractableObject in _selectVillageInteractableObjects)
            {
                selectVillageInteractableObject.Click += SelectVillageInteractable_Click;
            }
        }

        public void SwitchInputState(bool state)
        {
            _villageInteractableObject.SwitchObjectState(state);
        }

        public void SwitchVillageInteractableObjectsState(bool state)
        {
            foreach (var interactableObject in _buildings)
            {
                interactableObject.SwitchObjectState(state);
            }
            
            foreach (var selectVillageInteractableObject in _selectVillageInteractableObjects)
            {
                selectVillageInteractableObject.SwitchObjectState(state);
            }
        }

        private void SelectVillageInteractable_Click()
        {
            SwitchVillageInteractableObjectsState(false);
        }
    }
}
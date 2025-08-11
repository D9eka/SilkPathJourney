using Internal.Scripts.InteractableObjects;
using UnityEngine;

namespace Internal.Scripts
{
    public class Village : MonoBehaviour
    {
        [SerializeField] private Vector2 _cameraPosition;
        [SerializeField] private InteractableObject _villageInteractableObject;
        [SerializeField] private InteractableObject[] _selectVillageInteractableObjects;
        [SerializeField] private InteractableObject[] _buildings;

        public Vector2 CameraPosition => _cameraPosition;
        public InteractableObject VillageInteractableObject => _villageInteractableObject;
        public InteractableObject[] SelectVillageInteractableObjects => _selectVillageInteractableObjects;
        public InteractableObject[] Buildings => _buildings;

        private void Awake()
        {
            _villageInteractableObject.SwitchObjectState(false);
        }

        public void SwitchInputState(bool state) =>
            _villageInteractableObject.SwitchObjectState(state);

        public void SwitchVillageInteractableObjectsState(bool state)
        {
            foreach (var building in _buildings)
                building.SwitchObjectState(state);

            foreach (var selectVillage in _selectVillageInteractableObjects)
                selectVillage.SwitchObjectState(state);
        }
    }
}
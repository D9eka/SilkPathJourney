using Internal.Scripts.InteractableObjects;
using UnityEngine;

namespace Internal.Scripts.World.Village
{
    public class Village : MonoBehaviour
    {
        [field:SerializeField] public Vector2 CameraPosition { get; private set; }
        [field:SerializeField] public InteractableObject VillageInteractableObject { get; private set; }
        [field:SerializeField] public InteractableObject[] SelectVillageInteractableObjects { get; private set; }
        [field:SerializeField] public InteractableObject[] Buildings { get; private set; }
        [field:SerializeField] public int RoadNodeIndex { get; private set; }

        private void Awake()
        {
            VillageInteractableObject.SwitchObjectState(false);
        }

        public void SwitchInputState(bool state) =>
            VillageInteractableObject.SwitchObjectState(state);

        public void SwitchVillageInteractableObjectsState(bool state)
        {
            foreach (InteractableObject building in Buildings)
                building.SwitchObjectState(state);

            foreach (InteractableObject selectVillage in SelectVillageInteractableObjects)
                selectVillage.SwitchObjectState(state);
        }
    }
}
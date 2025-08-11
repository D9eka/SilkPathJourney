using Internal.Scripts.Camera;
using Internal.Scripts.InteractableObjects;

namespace Internal.Scripts
{
    public class MoverController
    {
        private CameraController _cameraController;

        private Village[] _villages;
        private Village _currentVillage;

        private bool _isVillageSelectionStarted;

        public MoverController(CameraController cameraController, Village[] villages, Village currentVillage)
        {
            _cameraController = cameraController;

            _villages = villages;
            _currentVillage = currentVillage;
            EnableCurrentVillage();
            foreach (Village village in _villages)
            {
                village.VillageInteractableObject.Click += VillageInteractableObject_Click;
                foreach (SelectVillageInteractableObject villageSelectVillageInteractableObject in village
                             .SelectVillageInteractableObjects)
                {
                    villageSelectVillageInteractableObject.Click += SelectVillageInteractableObject_Click;
                }
            }
        }

        private void VillageInteractableObject_Click(Village village)
        {
            if (_isVillageSelectionStarted)
            {
                StartMovingToVillage(village);
            }
        }

        private void SelectVillageInteractableObject_Click()
        {
            SelectVillage();
        }

        private void SelectVillage()
        {
            if (_isVillageSelectionStarted)
            {
                return;
            }

            _isVillageSelectionStarted = true;
            _cameraController.ZoomCamera(219, () =>
            {
                SwitchVillagesState(true);
            });
        }

        private void SwitchVillagesState(bool state)
        {
            foreach (Village village in _villages)
            {
                village.SwitchInputState(state);
            }
        }

        private void StartMovingToVillage(Village village)
        {
            _isVillageSelectionStarted = false;
            SwitchVillagesState(false);
            _currentVillage = village;

            _cameraController.ZoomCamera(17, () =>
                _cameraController.MoveCamera(village.CameraPosition, EnableCurrentVillage));
        }

        private void EnableCurrentVillage()
        {
            foreach (Village village in _villages)
            {
                village.SwitchVillageInteractableObjectsState(village == _currentVillage);
            }
        }
    }
}
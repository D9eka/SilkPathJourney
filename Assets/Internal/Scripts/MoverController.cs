using System.Collections.Generic;
using Internal.Scripts.Camera;
using Internal.Scripts.InteractableObjects;
using UnityEngine;

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
            
            foreach (Village village in _villages)
            {
                village.SwitchVillageInteractableObjectsState(false);
            }
            
            Transform[] villagesToFocus = GetVillagesToFocus();
            _cameraController.FocusOnObjects(villagesToFocus, () =>
            {
                SwitchVillagesState(true);
            });
        }

        private Transform[] GetVillagesToFocus()
        {
            List<Transform> villagesToFocus = new List<Transform>();
            for (int i = 0; i < _villages.Length; i++)
            {
                if (_villages[i] == _currentVillage)
                {
                    villagesToFocus.Add(_villages[i].transform);
                    if (i > 0)
                    {
                        villagesToFocus.Add(_villages[i - 1].transform);
                    }
                    if (i < _villages.Length - 1)
                    {
                        villagesToFocus.Add(_villages[i + 1].transform);
                    }
                }
            }

            return villagesToFocus.ToArray();
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
            
            _cameraController.FocusOnObjects(new [] { _currentVillage.transform }, () =>
            {
                _currentVillage = village;
                _cameraController.MoveCamera(_currentVillage.CameraPosition, EnableCurrentVillage);
            });
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
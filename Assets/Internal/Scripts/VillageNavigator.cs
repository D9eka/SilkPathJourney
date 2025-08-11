using System;
using System.Collections.Generic;
using Internal.Scripts.Camera;
using Internal.Scripts.InteractableObjects;
using UnityEngine;
using Zenject;

namespace Internal.Scripts
{
    public class VillageNavigator : IInitializable
    {
        private readonly CameraController _cameraController;
        private readonly Village[] _villages;
        private Village _currentVillage;
        private bool _isVillageSelectionInProgress;

        [Inject]
        public VillageNavigator(
            CameraController cameraController,
            Village[] villages,
            [Inject(Id = "CurrentVillage")] Village currentVillage)
        {
            _cameraController = cameraController;
            _villages = villages;
            _currentVillage = currentVillage;
        }

        public void Initialize()
        {
            EnableCurrentVillageInteractables();

            foreach (Village village in _villages)
            {
                village.VillageInteractableObject.OnClick += _ => OnVillageSelected(village);

                foreach (InteractableObject interactableObject in village.SelectVillageInteractableObjects)
                {
                    interactableObject.OnClick += _ => EnterVillageSelectionMode();
                }
            }
        }

        private void OnVillageSelected(Village village)
        {
            TransitionToVillage(village);
        }

        private void EnterVillageSelectionMode()
        {
            if (_isVillageSelectionInProgress)
            {
                return;
            }

            _isVillageSelectionInProgress = true;

            foreach (var village in _villages)
            {
                village.SwitchVillageInteractableObjectsState(false);
            }

            Transform[] targets = GetNearbyVillagesForFocus();
            _cameraController.FocusOnObjects(targets, () => SetVillagesInputState(true));
        }

        private Transform[] GetNearbyVillagesForFocus()
        {
            int currentIndex = Array.IndexOf(_villages, _currentVillage);
            if (currentIndex < 0)
            {
                return Array.Empty<Transform>();
            }

            List<Transform> targets = new List<Transform>
            {
                _villages[currentIndex].transform
            };

            if (currentIndex > 0)
            {
                targets.Add(_villages[currentIndex - 1].transform);
            }

            if (currentIndex < _villages.Length - 1)
            {
                targets.Add(_villages[currentIndex + 1].transform);
            }

            return targets.ToArray();
        }

        private void SetVillagesInputState(bool isEnabled)
        {
            foreach (var village in _villages)
            {
                village.SwitchInputState(isEnabled);
            }
        }

        private void TransitionToVillage(Village targetVillage)
        {
            _isVillageSelectionInProgress = false;
            SetVillagesInputState(false);

            _cameraController.FocusOnObjects(new[] { _currentVillage.transform }, () =>
            {
                _currentVillage = targetVillage;
                _cameraController.MoveCamera(_currentVillage.CameraPosition, EnableCurrentVillageInteractables);
            });
        }

        private void EnableCurrentVillageInteractables()
        {
            foreach (var village in _villages)
            {
                village.SwitchVillageInteractableObjectsState(village == _currentVillage);
            }
        }
    }
}
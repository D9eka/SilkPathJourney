using System;
using Internal.Scripts.InteractableObjects;
using Internal.Scripts.World.Camera;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace Internal.Scripts.Input
{
    public class InputManager : IInitializable, ITickable, IDisposable
    {
        private readonly UnityEngine.Camera _mainCamera;
        private readonly ICameraRig _cameraRig;
        private PlayerInputActions _inputActions;

        private InteractableObject _currentHover;

        [Inject]
        public InputManager(UnityEngine.Camera mainCamera, ICameraRig cameraRig)
        {
            _mainCamera = mainCamera;
            _cameraRig = cameraRig;
        }

        public void Initialize()
        {
            _inputActions = new PlayerInputActions();
            EnableInput();
        }

        public void Dispose()
        {
            DisableInput();
        }

        public void Tick()
        {
            HandleHover();
        }

        private void EnableInput()
        {
            _inputActions.Enable();
            _inputActions.Player.Click.performed += OnClick;
            _inputActions.Player.ZoomCamera.performed += OnZoomCamera;
        }

        private void DisableInput()
        {
            _inputActions.Player.Click.performed -= OnClick;
            _inputActions.Player.ZoomCamera.performed -= OnZoomCamera;
            _inputActions.Disable();
        }

        private void HandleHover()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            RaycastHit2D hit = Physics2D.GetRayIntersection(_mainCamera.ScreenPointToRay(screenPos));

            if (hit.collider != null && hit.collider.TryGetComponent(out InteractableObject view))
            {
                if (_currentHover != view)
                {
                    _currentHover?.TriggerHoverExit();
                    _currentHover = view;
                    _currentHover.TriggerHoverEnter();
                }
            }
            else
            {
                _currentHover?.TriggerHoverExit();
                _currentHover = null;
            }
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            RaycastHit2D hit = Physics2D.GetRayIntersection(_mainCamera.ScreenPointToRay(screenPos));

            if (hit.collider != null && hit.collider.TryGetComponent(out InteractableObject view))
            {
                view.TriggerClick();
            }
        }
        
        private void OnZoomCamera(InputAction.CallbackContext obj)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;
            
            float value = obj.ReadValue<float>();
            _cameraRig.ChangeSize(value);
        }
    }
}

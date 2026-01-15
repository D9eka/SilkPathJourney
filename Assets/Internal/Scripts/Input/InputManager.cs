using System;
using Internal.Scripts.InteractableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace Internal.Scripts.Input
{
    public class InputManager : IInitializable, ITickable, IDisposable
    {
        private static readonly float CAMERA_SIZE_MODIFIER = -1f;
        private static readonly Vector2 CAMERA_MOVEMENT_MODIFIER = new Vector2(-1f, -1f);

        public Action<float> OnChangeCameraSize;
        public Action<Vector2> OnChangeCameraPosition;
        
        private readonly UnityEngine.Camera _mainCamera;
        private PlayerInputActions _inputActions;

        private InteractableObject _currentHover;

        [Inject]
        public InputManager(UnityEngine.Camera mainCamera)
        {
            _mainCamera = mainCamera;
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
            _inputActions.Player.MoveCamera.started += OnMoveCamera;
            _inputActions.Player.MoveCamera.performed += OnMoveCamera;
            _inputActions.Player.MoveCamera.canceled += OnMoveCamera;
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
            OnChangeCameraSize?.Invoke(value * CAMERA_SIZE_MODIFIER);
        }
        
        private void OnMoveCamera(InputAction.CallbackContext obj)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;
            
            Vector2 value = obj.ReadValue<Vector2>();
            OnChangeCameraPosition?.Invoke(new Vector2(value.x, value.y) * CAMERA_MOVEMENT_MODIFIER);
        }
    }
}

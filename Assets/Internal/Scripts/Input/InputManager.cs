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
        private const float CAMERA_SIZE_MODIFIER = -1f;
        private const float MAX_RAY_DISTANCE = 1000f;
        
        private static readonly Vector2 CameraMovementModifier = new Vector2(-1f, -1f);

        public Action<float> OnChangeCameraSize;
        public Action<Vector2> OnChangeCameraPosition;
        
        private readonly UnityEngine.Camera _mainCamera;
        private readonly LayerMask _interactableLayerMask;
        private PlayerInputActions _inputActions;

        private IInteractableObject _currentHover;

        private bool _clickRequested;
        private float _zoomValue;
        private Vector2 _moveValue;

        [Inject]
        public InputManager(UnityEngine.Camera mainCamera, LayerMask interactableLayerMask)
        {
            _mainCamera = mainCamera;
            _interactableLayerMask = interactableLayerMask;
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
            bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            HandleHover(isOverUI);
            HandleClick(isOverUI);
            HandleCamera(isOverUI);

            _clickRequested = false;
            _zoomValue = 0;
        }

        private void EnableInput()
        {
            _inputActions.Enable();

            _inputActions.Player.Click.performed += OnClick;

            _inputActions.Player.ZoomCamera.performed += OnZoomCamera;
            _inputActions.Player.ZoomCamera.canceled += OnZoomCamera;

            _inputActions.Player.MoveCamera.performed += OnMoveCamera;
            _inputActions.Player.MoveCamera.canceled += OnMoveCamera;
        }

        private void DisableInput()
        {
            _inputActions.Player.Click.performed -= OnClick;
            _inputActions.Player.ZoomCamera.performed -= OnZoomCamera;
            _inputActions.Player.ZoomCamera.canceled -= OnZoomCamera;
            _inputActions.Player.MoveCamera.performed -= OnMoveCamera;
            _inputActions.Player.MoveCamera.canceled -= OnMoveCamera;

            _inputActions.Disable();
        }

        private void HandleHover(bool isOverUI)
        {
            if (isOverUI)
            {
                if (_currentHover != null)
                {
                    _currentHover.TriggerHoverExit();
                    _currentHover = null;
                }
                return;
            }

            IInteractableObject view = TryGetInteractableUnderMouse();

            if (_currentHover != view)
            {
                _currentHover?.TriggerHoverExit();
                _currentHover = view;
                _currentHover?.TriggerHoverEnter();
            }
        }

        private void HandleClick(bool isOverUI)
        {
            if (!_clickRequested || isOverUI)
                return;

            IInteractableObject view = TryGetInteractableUnderMouse();
            view?.TriggerClick();
        }

        private void HandleCamera(bool isOverUI)
        {
            if (isOverUI)
                return;
            
            OnChangeCameraSize?.Invoke(_zoomValue * CAMERA_SIZE_MODIFIER);
            OnChangeCameraPosition?.Invoke(_moveValue * CameraMovementModifier);
        }
        
        private IInteractableObject TryGetInteractableUnderMouse()
        {
            if (Mouse.current == null)
                return null;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, MAX_RAY_DISTANCE, _interactableLayerMask))
            {
                if (hit.collider.TryGetComponent(out IInteractableObject view))
                    return view;
            }

            return null;
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _clickRequested = true;
            }
        }

        private void OnZoomCamera(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _zoomValue = 0;
                return;
            }

            _zoomValue = context.ReadValue<float>();
        }

        private void OnMoveCamera(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _moveValue = Vector2.zero;
                return;
            }

            _moveValue = context.ReadValue<Vector2>();
        }
    }
}
